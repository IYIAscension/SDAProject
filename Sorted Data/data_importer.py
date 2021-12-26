import struct
from typing import List, Tuple, Optional


def list_countries() -> List[str]:
    """Returns a list of all countries for which data is available.

    Returns:
        List[str]: The list of country directory names.
    """
    output = []
    with open('countries.txt', 'r', encoding='utf8') as file:
        for line in file.readlines():
            line = line.strip()
            if len(line) < 1:
                continue
            output.append(line)

    return output


def import_dates(filepath: str) -> List[Tuple[int, int, int]]:
    """Reads the given file and interprets the binary data therein as a
    sequence of dates.

    Args:
        filepath (str): The path to the file to read.

    Returns:
        List[Tuple[int, int, int]]: The list of dates read from
        the file.
    """
    output = []
    fmt = "xBHBB"
    # First date in dataset is February 24th, 2020
    last_known_date = (2020, 2, 24)
    try:
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
                if has_value > 0:
                    last_known_date = (year, month, date)

                output.append(last_known_date)

        return output
    except ValueError:
        print(f'[import_dates] Invalid file path: {filepath}.')
        return None


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
            if has_value > 0:
                output.append(value)
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
            if has_value > 0:
                output.append(value)
            else:
                output.append(default)

    return output


def import_text(filepath) -> List[str]:
    with open(filepath, 'r') as file:
        return file.readlines()


def handle_none_time_series(data: List[Optional[float]]) -> List[float]:
    """Removes None values from a dataset by treating it as a time series.
    That is to say, a 'None' value means 'no change from previous observation'.

    Args:
        data (List[Optional[float]]): The dataset to remove None values from.

    Returns:
        List[float]: The resulting dataset.
    """
    last_data = 0.0
    new_data = list()
    for sample in data:
        if sample is not None:
            last_data = sample

        new_data.append(last_data)

    return new_data


def import_time_series(filepath: str) -> List[float]:
    """Reads a targeted file and interprets the bytes therein as a long list
    of 64-bit floating point numbers. Empty-marked values are set to the
    previously known value, or 0.0 for initial values.

    Args:
        filepath (str): The path to the file to read.

    Returns:
        List[float]: The time series that was read.
    """
    output = []
    # Total size needs to be aligned to 16 bytes.
    # Data is 9 bytes -> 7 bytes of padding.
    fmt = "xxxxxxxBd"
    padding = b'\00\00\00\00\00\00\00'
    last_value = 0.0
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
            if has_value > 0:
                last_value = value

            output.append(last_value)

    return output


def import_final(filepath: str) -> Optional[float]:
    """Reads a targeted file and interprets the bytes therein as a long list
    of 64-bit floating point numbers. Empty-marked values are set to the
    previously known value, or 0.0 for initial values. Returns the last value
    in the sequence.

    Args:
        filepath (str): The path to the file to read.

    Returns:
        Optional[float]: The final value that was read, or None if no data is
        in the set.
    """
    # Total size needs to be aligned to 16 bytes.
    # Data is 9 bytes -> 7 bytes of padding.
    fmt = "xxxxxxxBd"
    padding = b'\00\00\00\00\00\00\00'
    last_value = None
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
            if has_value > 0:
                last_value = value

    return last_value


def date_equal(a: Tuple[int, int, int], b: Tuple[int, int, int]) -> bool:
    """Returns true if the two given dates are equal."""
    return a is not None and b is not None and \
        a[0] == b[0] and a[1] == b[1] and a[2] == b[2]


def limit_by_date(dates: List[Tuple[int, int, int]],
                  start: Tuple[int, int, int],
                  stop: Tuple[int, int, int]) -> Tuple[int, int]:
    """Given a list of dates, and a start and end date, returns a tuple of
    indices to slice by to limit datasets to the given range of dates.

    Args:
        dates (List[Tuple[int, int, int]]): The list of dates.
        start (Tuple[int, int, int]): The starting date (inclusive).
        stop (Tuple[int, int, int]): The stopping date (inclusive).

    Returns:
        Tuple[int, int]: The tuple of indices.
    """
    out_start = 0
    out_end = len(dates) - 1
    for i in range(out_end + 1):
        date = dates[i]
        if date_equal(date, start):
            out_start = i
        if date_equal(date, stop):
            out_end = i

    return (out_start, out_end)


# Demo of usage.
if __name__ == "__main__":
    dates = import_dates('Netherlands/date.data')
    data = import_numerics_handle_none("Netherlands/total_deaths.data", 0.0)
    print('Demo: Netherlands Covid-19 deaths by date.')
    for i in range(100):
        (y, m, d) = dates[i]
        deaths = data[i]

        print(f'Date: {d:02}-{m:02}, {y:04}: {deaths:.1f} death(s).')
