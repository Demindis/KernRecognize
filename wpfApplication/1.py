import imutils
import easyocr
import cv2 as cv
import csv
from io import StringIO
import argparse
import numpy as np
import torch


def dist(center1, center2):
    return ((center1[0] - center2[0]) ** 2 + (center1[1] - center2[1]) ** 2) ** 0.5


def rotate_point(x, y, cx, cy, cx1, cy1, angle):
    angle = np.radians(angle)
    x_new = np.cos(angle) * (x - cx) - np.sin(angle) * (y - cy) + cx1
    y_new = np.sin(angle) * (x - cx) + np.cos(angle) * (y - cy) + cy1
    return [x_new, y_new]


def changeBox(elem, cx, cy, cx1, cy1, angle):
    bbox, text, prob = elem
    (tl, tr, br, bl) = bbox
    tl = rotate_point(int(tl[0]), int(tl[1]), cx, cy, cx1, cy1, -angle)
    br = rotate_point(int(br[0]), int(br[1]), cx, cy, cx1, cy1, -angle)
    center = [round((tl[0] + br[0]) / 2), round((tl[1] + br[1]) / 2)]
    radius = round(max(abs(tl[0] - br[0]), abs(tl[1] - br[1])) / 2)
    return [center, radius, text]


def boxInResult(box_center, box_number):
    for i in range(len(box_number)):
        if dist(box_center, box_number[i][0]) < box_number[i][1]:
            return i
    return -1


def recognize(img_path):
    reader = easyocr.Reader(['en'],
                            model_storage_directory='C:/Users/Admin/.EasyOCR/model',
                            user_network_directory='C:/Users/Admin/.EasyOCR/user_network',
                            recog_network='custom_example', gpu=True)

    img = cv.imread(img_path)

    height1, width1 = img.shape[:2]
    cx1, cy1 = width1 / 2, height1 / 2

    box_number = []

    for angle in range(0, 360, 10):
        rotated = imutils.rotate_bound(img, angle)

        height, width = rotated.shape[:2]
        cx, cy = width / 2, height / 2

        result = reader.readtext(rotated, allowlist='0123456789-')
        for elem in result:
            word = elem[1]
            if len(word) == 8:
                if word.count('-') == 1 and word[5] == '-':
                    if int(word[0:5]) > 10000 and int(word[0:5]) < 60000:
                        elem = changeBox(elem, cx, cy, cx1, cy1, angle)
                        id = boxInResult(elem[0], box_number)
                        if id == -1:
                            box_number.append([elem[0], elem[1], {elem[2]: 1}])
                        else:
                            if elem[2] in box_number[id][2]:
                                box_number[id][2][elem[2]] += 1
                            else:
                                box_number[id][2][elem[2]] = 1
        torch.cuda.empty_cache()

    for elem in box_number:
        max = 0
        for key in elem[2]:
            if elem[2][key] > max:
                number = key
                max = elem[2][key]
        elem[2] = number

    csv_data = []
    for elem in box_number:
        csv_data.append([str(elem[0][0]), str(elem[0][1]), str(elem[1]), elem[2]])

    return csv_data

if __name__ == "__main__":
    try:
        # Парсинг аргументов командной строки
        parser = argparse.ArgumentParser()
        parser.add_argument("image_path", help="Путь к изображению для распознавания")
        args = parser.parse_args()

        # Вызов функции распознавания и генерации CSV
        csv_data = recognize(args.image_path)

        # Вывод CSV данных в виде строк
        for row in csv_data:
            print(','.join(row))  # Печатаем каждую строку
    except Exception as ex:
        print(ex)
    input()
