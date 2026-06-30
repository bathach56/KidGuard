import '../../logs/domain/device_log_entry.dart';
import 'device_summary.dart';
import 'paired_device.dart';

abstract class DeviceRepository {
  Future<PairedDevice> pairDevice({
    required String accessToken,
    required String pairCode,
  });

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

  Future<List<DeviceLogEntry>> getDeviceLogs({
    required String accessToken,
    required String deviceId,
    int page = 1,
    int pageSize = 20,
  });
}
