import cv2
import mediapipe as mp
import socket
import json
import time
import threading
from queue import Queue, Empty

# Initialize MediaPipe with lower complexity settings
mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(
    max_num_faces=1,  # Only track one face
    refine_landmarks=False,  # Disable refinement for speed
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

# Socket configuration
server_address = ("127.0.0.1", 5005)
data_queue = Queue(maxsize=2)  # Queue to pass data to network thread

# Reduce camera resolution for faster processing
cap = cv2.VideoCapture(0)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

# Key landmarks - using only essential points
key_landmarks = [0, 4, 33, 263, 61, 291]  # Minimal set for head pose

# Network thread function
def network_thread():
    sock = None
    while True:
        try:
            # Connect if needed
            if sock is None:
                sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                sock.connect(server_address)
                print("Connected to C# application")

            # Get data from queue (non-blocking)
            try:
                data = data_queue.get(block=True, timeout=0.1)
                sock.sendall(data.encode())
                data_queue.task_done()
            except Empty:
                pass

        except socket.error as e:
            print(f"Socket error: {e}")
            if sock:
                sock.close()
                sock = None
            time.sleep(1.0)  # Reconnection delay

# Start network thread
net_thread = threading.Thread(target=network_thread, daemon=True)
net_thread.start()

# FPS calculation
prev_frame_time = 0
curr_frame_time = 0
show_fps = True

# Main loop
running = True
while cap.isOpened() and running:
    # Calculate FPS
    curr_frame_time = time.time()
    elapsed = curr_frame_time - prev_frame_time

    # Process at ~30 FPS
    if elapsed > 0.033:
        ret, frame = cap.read()
        if not ret:
            break

        # Process with MediaPipe (don't convert color space if not needed)
        frame.flags.writeable = False  # Pass by reference for performance
        results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
        frame.flags.writeable = True

        # Only extract and send data if landmarks detected
        if results.multi_face_landmarks:
            landmarks = results.multi_face_landmarks[0]
            all_landmarks = []

            for idx in key_landmarks:
                landmark = landmarks.landmark[idx]
                all_landmarks.append({
                    "id": idx,
                    "x": round(landmark.x, 3),  # Reduce precision
                    "y": round(landmark.y, 3),
                    "z": round(landmark.z, 3)
                })

            # Put data in queue for network thread (non-blocking)
            package = json.dumps({"landmarks": all_landmarks})
            try:
                data_queue.put_nowait(package)  # Don't block if queue full
            except:
                pass  # Skip this frame if queue full

        # Calculate and display FPS
        if show_fps:
            fps = 1 / (curr_frame_time - prev_frame_time)
            cv2.putText(frame, f"FPS: {int(fps)}", (10, 30),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)

        prev_frame_time = curr_frame_time

        # Show frame
        cv2.imshow("Face Tracker", frame)

    # Check for quit
    if cv2.waitKey(1) & 0xFF == ord("q"):
        running = False

# Clean up
cap.release()
cv2.destroyAllWindows()
print("Face tracking stopped")