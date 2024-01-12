import socket


def server_program():
    # get the hostname
    # host = "192.168.137.35"
    host = socket._LOCALHOST_V6
    print(host)
    port = 22222  # initiate port no above 1024
    data_buffer_len = 4096

    server_socket = socket.socket(socket.AF_INET6)  # get instance
    # look closely. The bind() function takes tuple as argument
    server_socket.bind((host, port))  # bind host address and port together
    print("Server Init")

    # configure how many client the server can listen simultaneously
    server_socket.listen(10)
    conn, address = server_socket.accept()  # accept new connection
    print("Connection from: " + str(address))
    while True:
        # receive data stream. it won't accept data packet greater than 1024 bytes
        data = conn.recv(data_buffer_len).decode()
        if (not data) or data == '<EOF>':
            # if data is not received break
            break
        print("from connected user: " + str(data))
        data = input(' -> ')
        conn.send(data.encode())  # send data to the client

    conn.close()  # close the connection


class CommunicateServer(object):
    def __init__(self, server_args):
        self.host = server_args.host
        self.port = server_args.port
        self.buf = server_args.buffer_len

    def get_connect(self):
        server_socket = socket.socket(socket.AF_INET)  # get instance
        # look closely. The bind() function takes tuple as argument
        server_socket.bind((self.host, self.port))  # bind host address and port together
        print("Server Init")

        # configure how many client the server can listen simultaneously
        server_socket.listen(10)
        conn, address = server_socket.accept()  # accept new connection
        print("Connection from: " + str(address))
        return conn


if __name__ == '__main__':
    server_program()
