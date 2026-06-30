import 'device_summary.dart';

abstract class DeviceRepository {
  Future<List<DeviceSummary>> getDevices({required String accessToken});
}
