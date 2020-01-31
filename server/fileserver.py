#!/usr/bin/python
import BaseHTTPServer
import os

HOST = '10.200.32.131'
PORT = 8088

class Handler(BaseHTTPServer.BaseHTTPRequestHandler):

    def do_PUT(s):
        length = int(s.headers['Content-Length'])
        path = "Storage/{}/".format(s.headers['Session-Id'])

        if not os.path.isdir(path):
            os.mkdir(path)

        with open(path + s.headers['File-Name'], "wb") as dst:
            dst.write(s.rfile.read(length))
        s.send_response(200)
        s.end_headers()
# Colors
class bcolors:
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'


if __name__ == '__main__':
    if not os.path.isdir('Storage'):
        os.mkdir('Storage')
    server_class = BaseHTTPServer.HTTPServer
    httpd = server_class((HOST, PORT), Handler)

    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print('{}\n[!] Server is terminated{}'.format(bcolors.WARNING, bcolors.ENDC))
        httpd.server_close()
