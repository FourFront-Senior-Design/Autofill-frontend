import io
import os

from os import listdir, mkdir
from os.path import isfile, isdir, join, exists
from google.oauth2 import service_account
from google.cloud import vision
from google.cloud.vision import types
from google.protobuf.json_format import MessageToJson

def OCR(filePath):
    # This will change to something else in time. Ideally we have the credentials file somewhere secure
    # and on the network so that any PCs to which we are deployed to can run our program.
    keyPath = "C:\Python\FourFrontScripts\credentials\FourFront Senior Design-162aa2f1754e.json"
    os.environ["GOOGLE_APPLICATION_CREDENTIALS"]=keyPath
    
    client = vision.ImageAnnotatorClient()

    imagePath = filePath + "/ReferencedImages/"
    outputPath = filePath + "/GoogleVisionData/"
    
    if not os.path.isdir(outputPath):
        os.mkdir(outputPath)
    
    fileList = [f for f in listdir(imagePath) if isfile(join(imagePath, f))]
    for f in fileList:
        nameParts = f.split('.')
        outFilePath = outputPath + nameParts[0] + ".json"

        if nameParts[1] == "jpg":
            if not exists(outFilePath):
                imagePath = imagePath + "/" + f

                with io.open(imagePath, 'rb') as image_file:
                    content = image_file.read()

                image = types.Image(content=content)

                response = client.text_detection(image=image)

                outFile = open(outFilePath, 'w+')

                outFile.write(MessageToJson(response))
