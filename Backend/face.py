import cv2
import mediapipe as mp
import socket
import json
import time

# Initialize MediaPipe FaceMesh
mp_face_mesh = mp.solutions.face_mesh
mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
face_mesh = mp_face_mesh.FaceMesh(min_detection_confidence=0.5, min_tracking_confidence=0.5)

# Socket configuration
server_address = ("127.0.0.1", 5005)
reconnect_delay = 1.0  # Seconds between reconnection attempts
update_rate = 0.05  # Limit to 20 FPS (50ms between updates)

# Capture video
# FOR MAC: delete , cv2.CAP_DSHOW
cap = cv2.VideoCapture(0)

# Store the last detected head state
last_state = "NONE"
tilt_threshold = 0.15
last_update_time = time.time()

# Function to create and connect socket
def connect_to_server():
    try:
        new_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        new_sock.connect(server_address)
        print("Connected to C# application")
        return new_sock
    except socket.error as e:
        print(f"Connection failed: {e}")
        return None

# Try initial connection
sock = connect_to_server()

running = True
while cap.isOpened() and running:
    # Throttle the frame rate
    current_time = time.time()
    if current_time - last_update_time < update_rate:
        continue
    last_update_time = current_time

    ret, frame = cap.read()
    if not ret:
        break

    # Process frame with MediaPipe
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(rgb_frame)

    if results.multi_face_landmarks:
        all_landmarks = []
        # Only process the first face
        landmarks = results.multi_face_landmarks[0]

        # Select a subset of important landmarks instead of all 468
        key_landmarks = [0, 4, 8, 33, 61, 199, 263, 291, 199]
        for idx in key_landmarks:
            landmark = landmarks.landmark[idx]
            all_landmarks.append({
                "id": idx,
                "x": round(landmark.x, 4),
                "y": round(landmark.y, 4),
                "z": round(landmark.z, 4)
            })

        # Try to send data
        if sock:
            try:
                sentPackage = json.dumps({"landmarks": all_landmarks})
                sock.sendall(sentPackage.encode())
            except socket.error as e:
                print(f"Socket error: {e}")
                sock.close()
                sock = None  # Set to None to trigger reconnection

        # Try to reconnect if needed
        if sock is None:
            time.sleep(reconnect_delay)
            sock = connect_to_server()

    # Display frame
    cv2.imshow("Face Tracker", frame)
    if cv2.waitKey(1) & 0xFF == ord("q"):
        running = False

# Clean up
if sock:
    sock.close()
cap.release()
cv2.destroyAllWindows()
print("Face tracking stopped")