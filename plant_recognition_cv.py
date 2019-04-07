import json
from watson_developer_cloud import VisualRecognitionV3

visual_recognition = VisualRecognitionV3(
    '2018-03-19',
    iam_apikey='CjQoYzLwOKosFMRNtJxmJhEF-YFdW8nNSn0r24uioNge')

with open('C:/Users/Alexander/Desktop/olive_example.jpg', 'rb') as images_file:
    classes = visual_recognition.classify(
        images_file,
        threshold='0.3',
	classifier_ids='DefaultCustomModel_864806219').get_result()
print(json.dumps(classes, indent=2))