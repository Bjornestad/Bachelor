import cv2
import mediapipe as mp
import socket
import json
import time

# Initialize MediaPipe FaceMesh
mp_face_mesh = mp.solutions.face_mesh
mp_drawing = mp.solutions.drawing_utils  # Helper for drawing landmarks
mp_drawing_styles = mp.solutions.drawing_styles  # Optional styling
face_mesh = mp_face_mesh.FaceMesh(min_detection_confidence=0.5, min_tracking_confidence=0.5)

# Create a socket to send data to C#
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ("127.0.0.1", 5005)  # Localhost, Port 5005

# Capture video
# FOR MAC: delete , cv2.CAP_DSHOW 
cap = cv2.VideoCapture(0)

# Store the last detected head state
last_state = "NONE"
tilt_threshold = 0.15  # Adjust sensitivity

# Flag to stop the loop
running = True

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break

    # Convert to RGB and process landmarks
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(rgb_frame)

    if results.multi_face_landmarks:
        for landmarks in results.multi_face_landmarks:
            for idx, landmark in enumerate(landmarks.landmark):
                jsonLandmark = {
                    "id": idx,
                    "x": round(landmark.x,6),
                    "y": round(landmark.y,6),
                    "z": round(landmark.z,6)
                }
                sentPackage = json.dumps(jsonLandmark)
                try:
                    sock.sendto(sentPackage.encode(), server_address)
                except socket.error as e:
                    print("Socket error: ",e)
                print(sentPackage.encode())

    # Show the webcam feed
    cv2.imshow("Face Tracker", frame)

    # Check if 'q' is pressed to exit
    if cv2.waitKey(1) & 0xFF == ord("q"):
        running = False  # Stop the loop when 'q' is pressed

    if not running:
        break

cap.release()
cv2.destroyAllWindows()
