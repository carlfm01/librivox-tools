import argparse
import subprocess


def get_arguments():
    parser = argparse.ArgumentParser(
        description='Speech cutter based on the Windows Speech recognition.')
    parser.add_argument('-sentences', type=str,
                        help='Single sentence to cut, or comma separated list of sentences.')
    parser.add_argument(
        '-input', type=str, help='Input audio file to cut, only wav format supported.')
    parser.add_argument('-output', type=str,
                        help='Directory to save cutted audio.')
    parser.add_argument('-confidence', type=str,
                        help='Filters out similar sentences and low quality ones, recommended to use 0,65.')
    parser.add_argument('-lang', type=str,
                        help='Language that the Windows speech recognition will use. Supported languages: English (United States, United Kingdom, Canada, India, and Australia), French, German, Japanese, Mandarin (Chinese Simplified and Chinese Traditional), and Spanish. Please install the language pack using the region section in your Windows 10')
    arguments = parser.parse_args()
    return arguments


if __name__ == '__main__':
    args = get_arguments()
    print(args.sentences)
    cmd = 'Speech.Cutter.Console.exe --sentences "'"{0}"'" --input {1} --output {2} --confidence {3} --lang {4}'.format(
        args.sentences, args.input, args.output, args.confidence, args.lang)
    process = subprocess.Popen(cmd, stderr=subprocess.PIPE, shell=True)
    process.wait()
    while True:
        rc = process.poll()
        if rc == 0:
            break
        output = process.stdout.read(1)
        if output == '':
            break
        if output:
            print(output.strip())
print('Completed')
