import os
import sys
import cv2
import mediapipe as mp
import socket
import json
import time
import threading
import statistics
import logging
from queue import Queue, Empty

pycache_dir = os.path.join(os.path.dirname(__file__), '__pycache__')
os.makedirs(pycache_dir, exist_ok=True)
logging.basicConfig(
    filename=os.path.join(pycache_dir, 'face_performance.log'),
    level=logging.INFO,
    format='%(asctime)s - %(message)s'
)

# Initialize MediaPipe with lower complexity settings
debug = True
debug_level = 2
mp_drawing = mp.solutions.drawing_utils
mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(
    max_num_faces=1,  # Only track one face
    refine_landmarks=False,  # Disable refinement for speed
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)
frame_counter = 0

# Socket configuration
server_address = ("127.0.0.1", 5005)
data_queue = Queue(maxsize=2)  # Queue to pass data to network thread

print("OS PLATFORM:", sys.platform)
if sys.platform == "darwin":
    cap = cv2.VideoCapture(0)
elif sys.platform.startswith("linux"):
    cap = cv2.VideoCapture(0)
else:
    cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

if sys.platform == "win32":
    timer_func = time.perf_counter  
else:
    timer_func = time.time


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

# Performance monitoring
frame_times = []
processing_times = []
transfer_times = []
landmark_times = []
camera_frame_times = []  # Track camera frame intervals
last_frame_timestamp = 0

# Main loop
running = True
while cap.isOpened() and running:
    # Start frame timer
    frame_start = timer_func()

    ret, frame = cap.read()
    if not ret:
        break

    # Measure camera frame interval
    current_time = timer_func()
    if last_frame_timestamp > 0:
        camera_delay = current_time - last_frame_timestamp
        camera_frame_times.append(camera_delay)
        if len(camera_frame_times) > 100:
            camera_frame_times.pop(0)
    last_frame_timestamp = current_time

    # Start processing timer
    process_start = timer_func()

    # Process with MediaPipe
    frame.flags.writeable = False
    results = face_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    frame.flags.writeable = True

    # Record processing time
    process_time = timer_func() - process_start
    processing_times.append(process_time)

    # Send facial landmark data only when detected
    if results.multi_face_landmarks:
        # Start landmark extraction timer
        landmark_start = timer_func()

        landmarks = results.multi_face_landmarks[0]

        # Get reference points for normalization
        nose_tip = landmarks.landmark[4]
        left_eye = landmarks.landmark[263]
        right_eye = landmarks.landmark[33]

        # Calculate reference distance (distance between eyes)
        eye_distance = ((left_eye.x - right_eye.x) ** 2 +
                        (left_eye.y - right_eye.y) ** 2 +
                        (left_eye.z - right_eye.z) ** 2) ** 0.5

        # Prevent division by zero
        if eye_distance < 0.001:
            eye_distance = 0.001

        # Create normalized data, using "#" to show which is actually used xd
        processed_data = {
            "rEyeCornerY": round(landmarks.landmark[33].y / eye_distance, 3),  #
            "lEyeCornerY": round(landmarks.landmark[263].y / eye_distance, 3),  #
            "rEyeCornerZ": round(landmarks.landmark[33].z / eye_distance, 3),  #
            "lEyeCornerZ": round(landmarks.landmark[263].z / eye_distance, 3),  #
            "lEyebrowY": round(landmarks.landmark[296].y / eye_distance, 3),  #
            "lEyesocketY": round(landmarks.landmark[450].y / eye_distance, 3),  #
            "rEyebrowY": round(landmarks.landmark[66].y / eye_distance, 3),  #
            "rEyesocketY": round(landmarks.landmark[230].y / eye_distance, 3),  #
            "MouthBotY": round(landmarks.landmark[17].y / eye_distance, 3),  #
            "MouthTopY": round(landmarks.landmark[0].y / eye_distance, 3),  #
            "MouthRX": round(landmarks.landmark[61].x / eye_distance, 3),  #
            "MouthLX": round(landmarks.landmark[291].x / eye_distance, 3),  #
            "rEarZ": round(landmarks.landmark[93].z / eye_distance, 3),  #
            "lEarZ": round(landmarks.landmark[323].z / eye_distance, 3),  #
            "ForeheadZ": round(landmarks.landmark[10].z / eye_distance, 3),  #
            "ChinZ": round(landmarks.landmark[152].z / eye_distance, 3)  #
        }

        # Record landmark extraction time
        landmark_times.append(timer_func() - landmark_start)

        # Start transfer timer
        transfer_start = timer_func()

        # Put data in queue for network thread
        try:
            package = json.dumps(processed_data)
            data_queue.put_nowait(package)

            # Record transfer time
            transfer_times.append(timer_func() - transfer_start)
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

    frame_time = timer_func() - frame_start
    frame_times.append(frame_time)

    if len(frame_times) > 100:
        frame_times.pop(0)
    if len(processing_times) > 100:
        processing_times.pop(0)
    if len(transfer_times) > 100:
        transfer_times.pop(0)
    if len(landmark_times) > 100:
        landmark_times.pop(0)

    frame_counter += 1

    if debug:
        if len(frame_times) > 0 and frame_counter % 30 == 0:
            avg_frame = statistics.mean(frame_times)
            avg_process = statistics.mean(processing_times)
            avg_transfer = statistics.mean(transfer_times) if transfer_times else 0
            avg_landmark = statistics.mean(landmark_times) if landmark_times else 0
            fps = 1.0 / avg_frame if avg_frame > 0 else 0

            # Only include camera timing metrics with debug_level >= 2
            performance_msg = f"Performance: FPS={fps:.1f}"

            if debug_level >= 2:
                cam_fps = "N/A"
                avg_camera = 0
                if len(camera_frame_times) > 0:
                    avg_camera = statistics.mean(camera_frame_times)
                    cam_fps = f"{(1.0 / avg_camera):.1f}" if avg_camera > 0 else "N/A"

                performance_msg += f", Camera={cam_fps}fps ({avg_camera * 1000:.1f}ms)"

            performance_msg += f", MediaPipe={avg_process * 1000:.1f}ms"

            if debug_level >= 1:
                performance_msg += f", Landmarks={avg_landmark * 1000:.1f}ms, Transfer={avg_transfer * 1000:.1f}ms"

            performance_msg += f", Queue={data_queue.qsize()}"

            print(performance_msg)

            # Always log complete info regardless of debug level
            logging.info(performance_msg)
            sys.stdout.flush()

    # Show frame (uncomment if needed)
    # cv2.imshow("Face Tracker", frame)

    # Check for quit
    if cv2.waitKey(1) & 0xFF == ord("q"):
        running = False

# Clean up
cap.release()
cv2.destroyAllWindows()
print("Face tracking stopped")