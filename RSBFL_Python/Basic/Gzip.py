import gzip
import base64

def gzip_compress(data):
    if type(data) == str:
        data = bytes(data, 'utf8')
    s_out = gzip.compress(data)
    base64_bytes = base64.b64encode(s_out)
    result = base64_bytes.decode('ascii')
    return result


def gzip_decompress(data):
    base64_bytes = data.encode('ascii')
    data = base64.b64decode(base64_bytes)
    return gzip.decompress(data).decode('utf8')


if __name__ == '__main__':

    suc_string = '0100;1011;0010;0011;'
    suc_raw_string = gzip_compress(suc_string)
    suc_string = gzip_decompress(suc_raw_string)
    fal_string = '1011;1101;1010;1111;'
    fal_raw_string = gzip_compress(fal_string)

    the_string = '010001101010101000101010;010100110100110101010101;1101111001110001110010101' \
                 ';00101011010101010101100110101011010100101011010; '
    raw_string = gzip_compress(the_string)
    de_string = gzip_decompress(raw_string)
    print("测试是否通过? {}".format(the_string == de_string))


