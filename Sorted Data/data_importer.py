import struct
from typing import List, Tuple, Optional


def import_dates(filepath: str) -> List[Optional[Tuple[int, int, int]]]:
    """Reads the given file and interprets the binary data therein as a
    sequence of dates.

    Args:
        filepath (str): The path to the file to read.

    Returns:
        List[Optional[Tuple[int, int, int]]]: The list of dates read from
        the file.
    """
    output = []
    fmt = "xBHBB"
    with open(filepath, 'rb') as file:
        bytes = file.read()

        # Each date object is 5 bytes.
        segments = int(len(bytes) / 5)
        for segment in range(segments):
            block_start = segment * 5
            block_end = block_start + 5
            has_value, year, month, date = struct.unpack(
                fmt,
                b'\00' + bytes[block_start:block_end]
            )
            if has_value:
                output.append((year, month, date))
            else:
                output.append(None)

    return output


def import_numerics(filepath: str) -> List[Optional[float]]:
    """Reads a targeted file and interprets the bytes therein as a long list
    of 64-bit floating point numbers. Empty-marked values are set to
    None in the output list.

    Args:
        filepath (str): The path to the file to read.

    Returns:
        List[Optional[float]]: The list of data that was read.
    """
    output = []
    # Total size needs to be aligned to 16 bytes.
    # Data is 9 bytes -> 7 bytes of padding.
    fmt = "xxxxxxxBd"
    padding = b'\00\00\00\00\00\00\00'
    with open(filepath, 'rb') as file:
        bytes = file.read()

        # Each data object is 9 bytes.
        segments = int(len(bytes) / 9)
        for segment in range(segments):
            block_start = segment * 9
            block_end = block_start + 9
            has_value, value = struct.unpack(
                fmt,
                padding + bytes[block_start:block_end]
            )
            if has_value:
                # For some reason Python makes them a factor of 10 bigger.
                # That, or C# did it. Either way, fix this data skewing.
                # Either Microsoft ff-ed up the double parser or there's
                # some inter-language weirdness at play.
                output.append(value * 0.1)
            else:
                output.append(None)

    return output


def import_numerics_handle_none(filepath: str, default: float) -> List[float]:
    """Reads a targeted file and interprets the bytes therein as a long list
    of 64-bit floating point numbers. Empty-marked values are set to
    the given default value.

    Args:
        filepath (str): The path to the file to read.

    Returns:
        List[float]: The list of data that was read.
    """
    output = []
    # Total size needs to be aligned to 16 bytes.
    # Data is 9 bytes -> 7 bytes of padding.
    fmt = "xxxxxxxBd"
    padding = b'\00\00\00\00\00\00\00'
    with open(filepath, 'rb') as file:
        bytes = file.read()

        # Each data object is 9 bytes.
        segments = int(len(bytes) / 9)
        for segment in range(segments):
            block_start = segment * 9
            block_end = block_start + 9
            has_value, value = struct.unpack(
                fmt,
                padding + bytes[block_start:block_end]
            )
            if has_value:
                # For some reason Python makes them a factor of 10 bigger.
                # That, or C# did it. Either way, fix this data skewing.
                # Either Microsoft ff-ed up the double parser or there's
                # some inter-language weirdness at play.
                output.append(value * 0.1)
            else:
                output.append(default)

    return output


def import_text(filepath) -> List[str]:
    with open(filepath, 'r') as file:
        return file.readlines()


# Demo of usage.
if __name__ == "__main__":
    dates = import_dates('Netherlands/date.data')
    data = import_numerics_handle_none("Netherlands/total_deaths.data", 0.0)
    print('Demo: Netherlands Covid-19 deaths by date.')
    for i in range(100):
        (y, m, d) = dates[i]
        deaths = data[i]

        print(f'Date: {d:02}-{m:02}, {y:04}: {deaths:.1f} death(s).')
