#!/usr/bin/env python
# coding: utf-8

import json
import re
from os import listdir
from os.path import isfile, join, splitext
import copy

import dataTemplate

def month_to_number(month_string):
    m = ''
    if month_string == 'JAN' or month_string == 'JANUARY':
        m = '01'
    elif month_string == 'FEB' or month_string == 'FEBRUARY':
        m = '02'
    elif month_string == 'MAR' or month_string == 'MARCH':
        m = '03'
    elif month_string == 'APR' or month_string == 'APRIL':
        m = '04'
    elif month_string == 'MAY':
        m = '05'
    elif month_string == 'JUN' or month_string == 'JUNE':
        m = '06'
    elif month_string == 'JUL' or month_string == 'JULY':
        m = '07'
    elif month_string == 'AUG' or month_string == 'AUGUST':
        m = '08'
    elif month_string == 'SEP' or month_string == 'SEPTEMBER':
        m = '09'
    elif month_string == 'OCT' or month_string == 'OCTOBER':
        m = '10'
    elif month_string == 'NOV' or month_string == 'NOVEMBER':
        m = '11'
    elif month_string == 'DEC' or month_string == 'DECEMBER':
        m = '12'
    return m


def replace_non_digits(input_string):
    input_string = input_string.replace('O', '0')
    input_string = input_string.replace('o', '0')
    input_string = input_string.replace('l', '1')
    input_string = input_string.replace('I', '1')
    input_string = input_string.replace('B', '8')
    return input_string

def extractDates(filePath):
    # regex to select dates
    re_dates = r'(JAN(?:UARY)?|FEB(?:RUARY)?|MAR(?:CH)?|APR(?:IL)?|MAY|JUN(?:E)?|JUL(?:Y)?|AUG(?:UST)?|SEP(?:TEMBER)?|OCT(?:OBER)?|NOV(?:EMBER)?|DEC(?:EMBER)?)\s+([\doOlI]{1,2})[,.]?\s+([\doOlI]{4})'

    dates = list()

    # set up date key list (dkl)
    dkl = ['BirthDate', 'DeathDate', 'BirthDateS_D', 'DeathDateS_D',
           'BirthDateS_D_2', 'DeathDateS_D_2', 'BirthDateS_D_3', 'DeathDateS_D_3',
           'BirthDateS_D_4', 'DeathDateS_D_4', 'BirthDateS_D_5', 'DeathDateS_D_5',
           'BirthDateS_D_6', 'DeathDateS_D_6']

    # print(filename + ':')
    with open(filePath, 'r') as file:
        data = json.load(file)
    extracted_text = data.get('textAnnotations')[0].get('description')
    # it_dates is an iterator over the search results
    it_dates = re.finditer(re_dates, extracted_text)
    dates = list()
    for date in it_dates:
        # print(date.groups())
        month = date.groups()[0]
        day = date.groups()[1]
        year = date.groups()[2]
        # convert month to numerical value
        month = month_to_number(month)

        # replace non-digits in day
        day = replace_non_digits(day)

        # replace non-digits in year
        year = replace_non_digits(year)

        # NOTE: We may not need the '/' separator here if we are populating the database
        # with this string, i.e. concatenated date such as 01012000 might work here
        # depending on their database date field settings
        new_date = str(month + '/' + day + '/' + year)
        dates.append(new_date)
        
    out_data = {}

    # put dates into out_data
    # if there is only one date, it goes into the DeathDate field, not the BirthDate field
    len_dates = len(dates)
    if len_dates == 0:
        pass
    elif len_dates % 2 == 0:
        for i in range(len_dates):
            out_data[dkl[i]] = dates[i]
    else:
        for j in range(len_dates - 1):
            out_data[dkl[j]] = dates[j]
            # this enters the last odd date into the DeathDate field (dkl[len_dates]), not BirthDate field
        out_data[dkl[len_dates]] = dates[len_dates - 1]
    
    return out_data
