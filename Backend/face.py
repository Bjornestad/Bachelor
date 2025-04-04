import sys

import cv2
import mediapipe as mp
import socket
import json
import time
import threading
mp_drawing = mp.solutions.drawing_utils
from queue import Queue, Empty

# Initialize MediaPipe with lower complexity settings
mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(
    max_num_faces=1,  # Only track one face
    refine_landmarks=False,  # Disable refinement for speed
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

debug = True

# Socket configuration
server_address = ("127.0.0.1", 5005)
data_queue = Queue(maxsize=2)  # Queue to pass data to network thread

# Reduce camera resolution for faster processing
print("OS PLATFORM: ",sys.platform)
if sys.platform=="darwin":
    cap = cv2.VideoCapture(0)
else:
    cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
#will need to setup a detection for this
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

#key_landmarks = [0,17,159,145,386,374,4,2,5,6,152,10]  
key_landmarks = [4, 33, 263, 296, 66, 230, 17, 0, 61, 291, 93, 323, 450]


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
                if isinstance(data, str):  # Text data (facial tracking)
                    sock.sendall(f"DATA:{data}\n".encode())
                elif isinstance(data, bytes):  # Image data
                    # Send image size first, then the bytes
                    sock.sendall(f"IMAGE:{len(data)}\n".encode())
                    sock.sendall(data)
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

        # Process with MediaPipe
        frame.flags.writeable = False
        results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
        frame.flags.writeable = True

        # Inside the main loop, after processing landmarks but before sending data:
        if results.multi_face_landmarks:
            landmarks = results.multi_face_landmarks[0]

            if debug:
                # Draw key landmarks
                h, w, c = frame.shape
                for idx in key_landmarks:
                    landmark = landmarks.landmark[idx]
                    x, y = int(landmark.x * w), int(landmark.y * h)
                    cv2.circle(frame, (x, y), 3, (0, 255, 0), -1)  # Green circle
                    cv2.putText(frame, str(idx), (x + 5, y + 5), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (255, 255, 255), 1)

                # Nose tip (used for X,Y,Z)
                nose_tip = landmarks.landmark[4]
                x, y = int(nose_tip.x * w), int(nose_tip.y * h)
                cv2.circle(frame, (x, y), 5, (0, 0, 255), -1)  # Red circle for nose tip

                # Draw measurement lines
                # Mouth height
                mouth_top = landmarks.landmark[0]
                mouth_bottom = landmarks.landmark[17]
                cv2.line(frame,
                         (int(mouth_top.x * w), int(mouth_top.y * h)),
                         (int(mouth_bottom.x * w), int(mouth_bottom.y * h)),
                         (255, 0, 0), 2)  # Blue line

                # Roll measurement line (left-right tilt)
                roll_left = landmarks.landmark[33]
                roll_right = landmarks.landmark[263]
                cv2.line(frame,
                         (int(roll_left.x * w), int(roll_left.y * h)),
                         (int(roll_right.x * w), int(roll_right.y * h)),
                         (0, 255, 255), 2)  # Yellow line

                # Left eyebrow height
                left_eye_top = landmarks.landmark[296]
                left_eyebrow = landmarks.landmark[450]
                cv2.line(frame,
                         (int(left_eye_top.x * w), int(left_eye_top.y * h)),
                         (int(left_eyebrow.x * w), int(left_eyebrow.y * h)),
                         (255, 0, 255), 2)  # Purple line

                # Right eyebrow height
                right_eye_top = landmarks.landmark[66]
                right_eyebrow = landmarks.landmark[230]
                cv2.line(frame,
                         (int(right_eye_top.x * w), int(right_eye_top.y * h)),
                         (int(right_eyebrow.x * w), int(right_eyebrow.y * h)),
                         (255, 0, 255), 2)  # Purple line

                # Mouth width
                mouth_left = landmarks.landmark[61]
                mouth_right = landmarks.landmark[291]
                cv2.line(frame,
                         (int(mouth_left.x * w), int(mouth_left.y * h)),
                         (int(mouth_right.x * w), int(mouth_right.y * h)),
                         (0, 165, 255), 2)  # Orange line

                # Head rotation
                rotation_left = landmarks.landmark[93]
                rotation_right = landmarks.landmark[323]
                cv2.line(frame,
                         (int(rotation_left.x * w), int(rotation_left.y * h)),
                         (int(rotation_right.x * w), int(rotation_right.y * h)),
                         (0, 128, 0), 2)  # Dark green line


        # Send facial landmark data only when detected
        if results.multi_face_landmarks:
            landmarks = results.multi_face_landmarks[0]

            processed_data = {
                "4x": round(landmarks.landmark[4].x, 3),
                "4y": round(landmarks.landmark[4].y, 3),
                "4z": round(landmarks.landmark[4].z, 3),
                "rEyeCornerY": round(landmarks.landmark[33].y, 3),
                "lEyeCornerY": round(landmarks.landmark[263].y, 3),
                "lEyebrowY": round(landmarks.landmark[296].y, 3),
                "lEyesocketY": round(landmarks.landmark[450].y, 3),
                "rEyebrowY": round(landmarks.landmark[66].y, 3),
                "rEyesocketY": round(landmarks.landmark[230].y , 3),
                "MouthBotY": round(landmarks.landmark[17].y, 3),
                "MouthTopY": round(landmarks.landmark[0].y, 3),
                "MouthRX": round(landmarks.landmark[61].x, 3),
                "MouthLX": round(landmarks.landmark[291].x, 3),
                "rEarZ": round(landmarks.landmark[93].z, 3),
                "lEarZ": round(landmarks.landmark[323].z, 3),
                
            }

            # Put data in queue for network thread (non-blocking)
            try:
                package = json.dumps(processed_data)
                data_queue.put_nowait(package)
            except:
                pass

        # Always send frames regardless of face detection
        if frame is not None:
            _, img_encoded = cv2.imencode('.jpg', frame, [int(cv2.IMWRITE_JPEG_QUALITY), 70])
            if _:
                try:
                    data_queue.put_nowait(img_encoded.tobytes())
                except Exception as e:
                    print(f"Failed to queue image: {e}")

        # Calculate and display FPS
        if show_fps:
            fps = 1 / (curr_frame_time - prev_frame_time)
            cv2.putText(frame, f"FPS: {int(fps)}", (10, 30),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)

        prev_frame_time = curr_frame_time

        #Show frame
        #cv2.imshow("Face Tracker", frame)

    # Check for quit
    if cv2.waitKey(1) & 0xFF == ord("q"):
        running = False

# Clean up
cap.release()
cv2.destroyAllWindows()
print("Face tracking stopped")