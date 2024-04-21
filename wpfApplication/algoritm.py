import imutils
import easyocr
import cv2 as cv
import csv
from io import StringIO
import argparse

def recognize(img_path):
    reader = easyocr.Reader(['en'],
                    model_storage_directory='C:/Users/Denis/.EasyOCR/model',
                    user_network_directory='C:/Users/Denis/.EasyOCR/user_network',
                    recog_network='custom_example', gpu=True)

    img = cv.imread(img_path)
    numbers = set()
    for ungle in range (0, 360, 10):
        rotated = imutils.rotate_bound(img, ungle)
        result = reader.readtext(rotated, allowlist='0123456789-')
        for elem in result:
            num = elem[1]
            if len(num) == 8:
                if num.count('-') == 1 and num[5] == '-':
                    if int(num[0:5]) > 10000 and int(num[0:5]) < 60000:
                        numbers.add(num)
    # Создание CSV строки
    csv_data = []
    for number in numbers:
        csv_data.append([number])

    return csv_data

if __name__ == "__main__":
    # Парсинг аргументов командной строки
    parser = argparse.ArgumentParser()
    parser.add_argument("image_path", help="Путь к изображению для распознавания")
    args = parser.parse_args()

    # Вызов функции распознавания и генерации CSV
    csv_data = recognize(args.image_path)

    # Вывод CSV данных в виде строк
    for row in csv_data:
        print(','.join(row))  # Печатаем каждую строку
