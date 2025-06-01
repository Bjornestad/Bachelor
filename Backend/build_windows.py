import PyInstaller.__main__
import os
import mediapipe

# Get MediaPipe package path to include its data files
mediapipe_path = os.path.dirname(mediapipe.__file__)

PyInstaller.__main__.run([
    'face.py',
    '--onefile',
    '--name=face',
    '--add-data', f'{mediapipe_path};mediapipe',
    '--hidden-import=mediapipe.python.solutions.face_mesh',
    '--hidden-import=mediapipe.python.solutions.drawing_utils',
])