import cv2
import socket
import json
import time
import keyboard
import mediapipe as mp


# Initialize MediaPipe FaceMesh
mp_face_mesh = mp.solutions.face_mesh
mp_drawing = mp.solutions.drawing_utils  # Helper for drawing landmarks
mp_drawing_styles = mp.solutions.drawing_styles  # Optional styling
face_mesh = mp_face_mesh.FaceMesh(min_detection_confidence=0.5, min_tracking_confidence=0.5)

# Create a socket to send data to C#
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ("127.0.0.1", 5005)  # Localhost, Port 5005

# Capture video
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)

# Store the last detected head state
last_state = "NONE"
tilt_threshold = 0.11  # Adjust sensitivity

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break

    # Convert to RGB and process landmarks
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(rgb_frame)

    if results.multi_face_landmarks:
        for landmarks in results.multi_face_landmarks:
            left_ear = landmarks.landmark[234]
            right_ear = landmarks.landmark[454]

            ear_difference = abs(left_ear.y - right_ear.y)

            # Determine head position
            if left_ear.y > right_ear.y + tilt_threshold:
                current_state = "RIGHT"
            elif right_ear.y > left_ear.y + tilt_threshold:
                current_state = "LEFT"
            else:
                current_state = "NONE"

            # Only output if state has changed
            if current_state != last_state:
                print(f"Head position changed: {current_state}")
                last_state = current_state  # Update last state
                if current_state == "RIGHT":
                    keyboard.press('q')
                    time.sleep(0.2) # these can be adjusted for the needed timing
                    keyboard.release('q')
                elif current_state == "LEFT":
                    keyboard.press('e')
                    time.sleep(0.2) # these can be adjusted for the needed timing
                    keyboard.release('e')
                elif current_state == "NONE" and last_state == "RIGHT":
                    keyboard.press('q')
                    time.sleep(0.2) # these can be adjusted for the needed timing
                    keyboard.release('q')
                elif current_state == "NONE" and last_state == "LEFT":
                    keyboard.press('e')
                    time.sleep(0.2) # these can be adjusted for the needed timing
                    keyboard.release('e')
    time.sleep(0.05) # Reduce polling frequency (adjust as needed)


            # Send command to C#
            # sock.sendto(json.dumps(command).encode(), server_address)

    cv2.imshow("Face Tracker", frame)
    if cv2.waitKey(1) & 0xFF == ord("å"):
        break

cap.release()
cv2.destroyAllWindows()
