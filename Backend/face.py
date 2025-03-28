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

debug = False

# Socket configuration
server_address = ("127.0.0.1", 5005)
data_queue = Queue(maxsize=2)  # Queue to pass data to network thread

# Reduce camera resolution for faster processing
cap = cv2.VideoCapture(0,cv2.CAP_DSHOW)
#cap = cv2.VideoCapture(0) my webcam needs DSHOW for some reason, todo figure out why
#will need to setup a detection for this
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

key_landmarks = [0,17,159,145,386,374,4,2,5,6,152,10]  
#0 top lip, 17 bottom lip, 
#159 top left eye, 145 top right eye, 386 bottom left eye, 374 bottom right eye
#4 nose tip, 2 nose bottom, 5 nose left, 6 nose right
#152 bottom chin, 10 top head


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
                sock.sendall((data + "\n").encode())
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

    if elapsed > 0.001:
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

            # In face.py, modify the processed_data creation:
            processed_data = {
                "X": round(landmarks.landmark[4].x, 3),  # Nose tip X 
                "Y": round(landmarks.landmark[4].y, 3),  # Nose tip Y
                "Z": round(landmarks.landmark[4].z, 3),
                "Roll": round(landmarks.landmark[33].y - landmarks.landmark[263].y, 3),
                "LeftEyebrowHeight": round(landmarks.landmark[296].y, 3),
                "RightEyebrowHeight": round(landmarks.landmark[105].y, 3),
                "MouthHeight": round(landmarks.landmark[17].y - landmarks.landmark[0].y, 3),
                "MouthWidth": round(abs(landmarks.landmark[61].x - landmarks.landmark[291].x), 3)
            }
            
            # Print landmark data if debug mode
            if debug:
                print("\n--- Key Landmark Data ---")
                for landmark in all_landmarks:
                    print(f"ID: {landmark['id']}, X: {landmark['x']}, Y: {landmark['y']}, Z: {landmark['z']}")

            # Put data in queue for network thread (non-blocking)
            try:
                package = json.dumps(processed_data)
                data_queue.put_nowait(package)
            except:
                pass

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