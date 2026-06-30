import 'device_summary.dart';

abstract class DeviceRepository {
  Future<List<DeviceSummary>> getDevices({required String accessToken});

  Future<DeviceSummary> getDevice({
    required String accessToken,
    required String deviceId,
  });

  Future<String> updateDeviceMode({
    required String accessToken,
    required String deviceId,
    required String mode,
  });
}
