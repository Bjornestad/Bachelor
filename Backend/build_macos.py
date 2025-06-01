import PyInstaller.__main__
import os
import mediapipe
import cv2

# Get current script directory
script_dir = os.path.dirname(os.path.abspath(__file__))
# Path to face.py
face_py_path = os.path.join(script_dir, 'face.py')
# Get MediaPipe package path to include its data files
mediapipe_path = os.path.dirname(mediapipe.__file__)
# Get OpenCV package path
opencv_path = os.path.dirname(cv2.__file__)

PyInstaller.__main__.run([
    face_py_path,
    '--onefile',
    '--name=face',
    f'--add-data={mediapipe_path}:mediapipe',
    '--hidden-import=mediapipe.python.solutions.face_mesh',
    '--hidden-import=mediapipe.python.solutions.drawing_utils',
    '--hidden-import=cv2',
    '--hidden-import=cv2.cv2',
])