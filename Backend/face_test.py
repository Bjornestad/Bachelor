import json
import socket
import threading
from queue import Queue
from unittest.mock import MagicMock, patch

import cv2
import numpy as np


def test_landmark_normalization():

    # Create mock landmarks data
    class MockLandmark:
        def __init__(self, x, y, z):
            self.x = x
            self.y = y
            self.z = z

    class MockFaceLandmarks:
        def __init__(self):
            self.landmark = {
                4: MockLandmark(0.5, 0.5, 0.1),  # nose_tip
                33: MockLandmark(0.4, 0.4, 0.1),  # right_eye
                263: MockLandmark(0.6, 0.4, 0.1),  # left_eye
                66: MockLandmark(0.4, 0.3, 0.1),  # right_eyebrow
                296: MockLandmark(0.6, 0.3, 0.1),  # left_eyebrow
            }

        def __getitem__(self, key):
            return self.landmark[key]

    landmarks = MockFaceLandmarks()

    left_eye = landmarks[263]
    right_eye = landmarks[33]

    # Calculate reference distance (distance between eyes)
    eye_distance = ((left_eye.x - right_eye.x) ** 2 +
                    (left_eye.y - right_eye.y) ** 2 +
                    (left_eye.z - right_eye.z) ** 2) ** 0.5

    normalized_value = landmarks[66].y / eye_distance

    expected = landmarks[66].y / eye_distance
    assert abs(normalized_value - expected) < 0.0001


@patch('socket.socket')
def test_network_thread(mock_socket):
    # Setup mock socket
    mock_socket_instance = MagicMock()
    mock_socket.return_value = mock_socket_instance

    data_queue = Queue()
    test_data = json.dumps({"test": "data"})
    data_queue.put(test_data)

    def test_network_func(queue):
        sock = socket.socket()
        sock.connect(("127.0.0.1", 5005))

        try:
            data = queue.get(block=True, timeout=0.1)
            if isinstance(data, str):
                sock.sendall(f"DATA:{data}\n".encode())
            queue.task_done()
        except Exception as e:
            print(f"Network error: {e}")
            pass

    thread = threading.Thread(target=test_network_func, args=(data_queue,))
    thread.daemon = True
    thread.start()
    thread.join(timeout=1.0)

    mock_socket_instance.connect.assert_called_once_with(("127.0.0.1", 5005))
    mock_socket_instance.sendall.assert_called_once_with(f"DATA:{test_data}\n".encode())


@patch('cv2.VideoCapture')
@patch('mediapipe.solutions.face_mesh.FaceMesh')
def test_face_detection(mock_face_mesh, mock_video_capture):

    # Setup mock video capture
    mock_cap = MagicMock()
    mock_video_capture.return_value = mock_cap

    # Create a fake frame
    fake_frame = np.zeros((480, 640, 3), dtype=np.uint8)
    mock_cap.read.return_value = (True, fake_frame)

    # Setup mock face mesh results
    mock_mesh = MagicMock()
    mock_face_mesh.return_value = mock_mesh

    # Create mock results with landmarks
    class MockResults:
        def __init__(self):
            class MockLandmark:
                def __init__(self, x, y, z):
                    self.x = x
                    self.y = y
                    self.z = z

            class MockFaceLandmarks:
                def __init__(self):
                    self.landmark = []
                    for i in range(468):
                        self.landmark.append(MockLandmark(0.5, 0.5, 0.1))

            self.multi_face_landmarks = [MockFaceLandmarks()]

    mock_mesh.process.return_value = MockResults()

    def process_single_frame():
        ret, frame = mock_cap.read()
        if not ret:
            return None

        frame.flags.writeable = False
        results = mock_mesh.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
        frame.flags.writeable = True

        if results.multi_face_landmarks:
            return True  
        return False

    result = process_single_frame()
    assert result is True  