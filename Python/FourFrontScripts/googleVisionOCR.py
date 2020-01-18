import io
import os
import sys

from os import listdir
from os.path import isfile, join, exists
from google.oauth2 import service_account
from google.cloud import vision
from google.cloud.vision import types
from google.protobuf.json_format import MessageToJson


def main():    
    # This will change to something else in time. Ideally we have the credentials file somewhere secure
    # and on the network so that any PCs to which we are deployed to can run our program.
    keyPath = "credentials\FourFront Senior Design-162aa2f1754e.json"
    os.environ["GOOGLE_APPLICATION_CREDENTIALS"]=keyPath
    
    filePath = ""
    outPath = ""
    
    client = vision.ImageAnnotatorClient()

    fileList = [f for f in listdir(filePath) if isfile(join(filePath, f))]
    for f in fileList:
        nameParts = f.split('.')
        outFilePath = outPath + "\\" + nameParts[0] + ".json"
        print("\n", nameParts[0], nameParts[1], end=" ")

        if nameParts[1] == "jpg":
            if not exists(outFilePath):
                imagePath = filePath + "\\" + f

                with io.open(imagePath, 'rb') as image_file:
                    content = image_file.read()

                image = types.Image(content=content)

                response = client.text_detection(image=image)

                outFile = open(outFilePath, 'w+')

                outFile.write(MessageToJson(response))

if __name__ == "__main__":
    main()
