#!/usr/bin/env python3
"""Run on the SERVER: python3 scripts/smoke_test.py http://127.0.0.1:5313
Requires only Python's standard library. Creates four actual calculation requests.
"""
import http.cookiejar
import json
import re
import sys
import time
import urllib.parse
import urllib.request
from html.parser import HTMLParser

class TokenParser(HTMLParser):
    token = None
    def handle_starttag(self, tag, attrs):
        values = dict(attrs)
        if tag == 'input' and values.get('name') == '__RequestVerificationToken':
            self.token = values.get('value')

base = (sys.argv[1] if len(sys.argv) > 1 else 'http://127.0.0.1:5313').rstrip('/')
client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
for operation, expected in [('Add', '10'), ('Subtract', '6'), ('Multiply', '16'), ('Divide', '4')]:
    with client.open(base + '/', timeout=15) as response:
        parser = TokenParser()
        parser.feed(response.read().decode())
    if not parser.token:
        raise SystemExit('FAIL: AntiForgery token not found.')
    payload = urllib.parse.urlencode({'Num1': '8', 'Num2': '2', 'Operation': operation,
                                     '__RequestVerificationToken': parser.token}).encode()
    with client.open(urllib.request.Request(base + '/Calculator/Calculate', data=payload), timeout=30) as response:
        result_url = response.geturl()
    request_id = urllib.parse.parse_qs(urllib.parse.urlsplit(result_url).query).get('id', [''])[0]
    if not request_id:
        request_id = urllib.parse.urlsplit(result_url).path.rstrip('/').split('/')[-1]
    if not re.fullmatch(r'[0-9a-fA-F-]{36}', request_id):
        raise SystemExit('FAIL: No correlation GUID in result redirect: ' + result_url)
    deadline = time.monotonic() + 60
    while True:
        with client.open(base + '/Calculator/Status?id=' + request_id, timeout=15) as response:
            result = json.load(response)
        if result['completed']:
            if result['result'] != expected or not result['databaseId']:
                raise SystemExit('FAIL: ' + str(result))
            print(f"PASS {operation}: result={result['result']} DB={result['databaseId']} GUID={request_id}")
            break
        if time.monotonic() > deadline:
            raise SystemExit('FAIL: consumer did not persist request within 60s: ' + request_id)
        time.sleep(1)
print('PASS: four HTTP -> Kafka consumer -> MariaDB requests completed.')
