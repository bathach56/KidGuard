class PairedDevice {
  const PairedDevice({
    required this.deviceId,
    required this.name,
    required this.mode,
    required this.deviceToken,
  });

  factory PairedDevice.fromJson(Object? json) {
    if (json is! Map<String, dynamic>) {
      throw const FormatException('Invalid pair device response.');
    }

    return PairedDevice(
      deviceId: json['deviceId']?.toString() ?? '',
      name:
          json['deviceName']?.toString() ??
          json['name']?.toString() ??
          'Windows Device',
      mode: json['mode']?.toString() ?? 'fun',
      deviceToken: json['deviceToken']?.toString() ?? '',
    );
  }

  final String deviceId;
  final String name;
  final String mode;
  final String deviceToken;
}
