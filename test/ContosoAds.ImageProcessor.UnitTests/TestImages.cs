// ReSharper disable RawStringCanBeSimplified

namespace ContosoAds.ImageProcessor.UnitTests;

internal static class TestImages
{
    internal const string Jpeg160By160 = """
                                         /9j/4AAQSkZJRgABAQAASABIAAD/4QBARXhpZgAATU0AKgAAAAgAAYdpAAQAAAABAAAAGgAAAAAAAqACAAQAAAABAAAAoKADAAQAAAABAAAAoAAAAAD/7QA4UGhvdG9zaG9wIDMuMAA4QklNBAQAAAAAAAA4QklNBCUAAAAAABDUHYzZjwCyBOmACZjs+EJ+/8AAEQgAoACgAwEiAAIRAQMRAf/EAB8AAAEFAQEBAQEBAAAAAAAAAAABAgMEBQYHCAkKC//EALUQAAIBAwMCBAMFBQQEAAABfQECAwAEEQUSITFBBhNRYQcicRQygZGhCCNCscEVUtHwJDNicoIJChYXGBkaJSYnKCkqNDU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6g4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2drh4uPk5ebn6Onq8fLz9PX29/j5+v/EAB8BAAMBAQEBAQEBAQEAAAAAAAABAgMEBQYHCAkKC//EALURAAIBAgQEAwQHBQQEAAECdwABAgMRBAUhMQYSQVEHYXETIjKBCBRCkaGxwQkjM1LwFWJy0QoWJDThJfEXGBkaJicoKSo1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoKDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uLj5OXm5+jp6vLz9PX29/j5+v/bAEMAAQEBAQEBAgEBAgICAgICAwICAgIDBAMDAwMDBAUEBAQEBAQFBQUFBQUFBQYGBgYGBgcHBwcHCAgICAgICAgICP/bAEMBAQEBAgICAwICAwgFBAUICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICP/dAAQACv/aAAwDAQACEQMRAD8A/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooA//9D+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigD//0f536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAP//S/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooA//9P+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigD//1P536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAP//V/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooA//9b+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigD//1/536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAP//Q/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooA//9k=
                                         """;

    internal const string Jpeg200By100 = """
                                         /9j/4AAQSkZJRgABAQAASABIAAD/4QBARXhpZgAATU0AKgAAAAgAAYdpAAQAAAABAAAAGgAAAAAAAqACAAQAAAABAAAAyKADAAQAAAABAAAAZAAAAAD/7QA4UGhvdG9zaG9wIDMuMAA4QklNBAQAAAAAAAA4QklNBCUAAAAAABDUHYzZjwCyBOmACZjs+EJ+/8AAEQgAZADIAwEiAAIRAQMRAf/EAB8AAAEFAQEBAQEBAAAAAAAAAAABAgMEBQYHCAkKC//EALUQAAIBAwMCBAMFBQQEAAABfQECAwAEEQUSITFBBhNRYQcicRQygZGhCCNCscEVUtHwJDNicoIJChYXGBkaJSYnKCkqNDU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6g4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2drh4uPk5ebn6Onq8fLz9PX29/j5+v/EAB8BAAMBAQEBAQEBAQEAAAAAAAABAgMEBQYHCAkKC//EALURAAIBAgQEAwQHBQQEAAECdwABAgMRBAUhMQYSQVEHYXETIjKBCBRCkaGxwQkjM1LwFWJy0QoWJDThJfEXGBkaJicoKSo1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoKDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uLj5OXm5+jp6vLz9PX29/j5+v/bAEMAAQEBAQEBAgEBAgICAgICAwICAgIDBAMDAwMDBAUEBAQEBAQFBQUFBQUFBQYGBgYGBgcHBwcHCAgICAgICAgICP/bAEMBAQEBAgICAwICAwgFBAUICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICP/dAAQADf/aAAwDAQACEQMRAD8A/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooA//9D+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigD//0f536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAP//S/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooA//9P+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigD//1P536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAP//V/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooA//9k=
                                         """;

    internal const string Jpeg100By200 = """
                                         /9j/4AAQSkZJRgABAQAASABIAAD/4QBARXhpZgAATU0AKgAAAAgAAYdpAAQAAAABAAAAGgAAAAAAAqACAAQAAAABAAAAZKADAAQAAAABAAAAyAAAAAD/7QA4UGhvdG9zaG9wIDMuMAA4QklNBAQAAAAAAAA4QklNBCUAAAAAABDUHYzZjwCyBOmACZjs+EJ+/8AAEQgAyABkAwEiAAIRAQMRAf/EAB8AAAEFAQEBAQEBAAAAAAAAAAABAgMEBQYHCAkKC//EALUQAAIBAwMCBAMFBQQEAAABfQECAwAEEQUSITFBBhNRYQcicRQygZGhCCNCscEVUtHwJDNicoIJChYXGBkaJSYnKCkqNDU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6g4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2drh4uPk5ebn6Onq8fLz9PX29/j5+v/EAB8BAAMBAQEBAQEBAQEAAAAAAAABAgMEBQYHCAkKC//EALURAAIBAgQEAwQHBQQEAAECdwABAgMRBAUhMQYSQVEHYXETIjKBCBRCkaGxwQkjM1LwFWJy0QoWJDThJfEXGBkaJicoKSo1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoKDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uLj5OXm5+jp6vLz9PX29/j5+v/bAEMAAQEBAQEBAgEBAgICAgICAwICAgIDBAMDAwMDBAUEBAQEBAQFBQUFBQUFBQYGBgYGBgcHBwcHCAgICAgICAgICP/bAEMBAQEBAgICAwICAwgFBAUICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICP/dAAQAB//aAAwDAQACEQMRAD8A/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooA//9D+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigD//0f536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKAP//S/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooA//9P+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigD//1P536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKAP//V/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooA//9b+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigD//1/536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKAP//Q/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooA//9H+d+iiiv8AtwP8vwooooAKKKKACiiigAooooAKKKKACiiigD//0v536KKK/wC3A/y/CiiigAooooAKKKKACiiigAooooAKKKKAP//T/nfooor/ALcD/L8KKKKACiiigAooooAKKKKACiiigAooooA//9k=
                                         """;
}