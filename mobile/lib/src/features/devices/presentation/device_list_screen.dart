import 'package:flutter/material.dart';

import '../domain/device_repository.dart';
import '../domain/device_summary.dart';
import 'device_detail_screen.dart';

class DeviceListScreen extends StatefulWidget {
  const DeviceListScreen({
    super.key,
    required this.accessToken,
    required this.deviceRepository,
  });

  final String accessToken;
  final DeviceRepository deviceRepository;

  @override
  State<DeviceListScreen> createState() => _DeviceListScreenState();
}

class _DeviceListScreenState extends State<DeviceListScreen> {
  late Future<List<DeviceSummary>> _devicesFuture;

  @override
  void initState() {
    super.initState();
    _devicesFuture = _loadDevices();
  }

  Future<List<DeviceSummary>> _loadDevices() {
    return widget.deviceRepository.getDevices(accessToken: widget.accessToken);
  }

  void _retry() {
    setState(() {
      _devicesFuture = _loadDevices();
    });
  }

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;

    return Scaffold(
      appBar: AppBar(title: const Text('Devices')),
      body: SafeArea(
        child: FutureBuilder<List<DeviceSummary>>(
          future: _devicesFuture,
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(child: CircularProgressIndicator());
            }

            if (snapshot.hasError) {
              return _DeviceListMessage(
                icon: Icons.cloud_off_outlined,
                title: 'Unable to load devices',
                message: 'Check the backend connection and try again.',
                action: OutlinedButton.icon(
                  onPressed: _retry,
                  icon: const Icon(Icons.refresh),
                  label: const Text('Retry'),
                ),
              );
            }

            final devices = snapshot.data ?? const <DeviceSummary>[];
            if (devices.isEmpty) {
              return const _DeviceListMessage(
                icon: Icons.devices_other_outlined,
                title: 'No paired devices',
                message: 'Pair a Windows computer to start monitoring.',
              );
            }

            return ListView.separated(
              padding: const EdgeInsets.all(20),
              itemCount: devices.length + 1,
              separatorBuilder: (_, index) =>
                  SizedBox(height: index == 0 ? 16 : 10),
              itemBuilder: (context, index) {
                if (index == 0) {
                  return Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Paired Devices',
                        style: textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(
                        'Select a computer to view status and protection mode.',
                        style: textTheme.bodyMedium?.copyWith(
                          color: Theme.of(context).colorScheme.onSurfaceVariant,
                        ),
                      ),
                    ],
                  );
                }

                return _DeviceTile(
                  device: devices[index - 1],
                  accessToken: widget.accessToken,
                  deviceRepository: widget.deviceRepository,
                );
              },
            );
          },
        ),
      ),
    );
  }
}

class _DeviceTile extends StatelessWidget {
  const _DeviceTile({
    required this.device,
    required this.accessToken,
    required this.deviceRepository,
  });

  final DeviceSummary device;
  final String accessToken;
  final DeviceRepository deviceRepository;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return ListTile(
      contentPadding: const EdgeInsets.all(16),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
        side: BorderSide(color: colorScheme.outlineVariant),
      ),
      leading: CircleAvatar(
        backgroundColor: device.isOnline
            ? colorScheme.primaryContainer
            : colorScheme.surfaceContainerHighest,
        foregroundColor: device.isOnline
            ? colorScheme.onPrimaryContainer
            : colorScheme.onSurfaceVariant,
        child: const Icon(Icons.computer_outlined),
      ),
      title: Text(device.name),
      subtitle: Padding(
        padding: const EdgeInsets.only(top: 6),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(device.computerName),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                StatusChip(
                  label: device.isOnline ? 'online' : 'offline',
                  icon: device.isOnline
                      ? Icons.wifi_tethering_outlined
                      : Icons.wifi_off_outlined,
                ),
                const StatusChip(label: 'mode', icon: Icons.shield_outlined),
                StatusChip(label: device.mode, icon: Icons.tune_outlined),
              ],
            ),
            const SizedBox(height: 8),
            Text(device.lastSeen),
          ],
        ),
      ),
      trailing: const Icon(Icons.chevron_right),
      onTap: () {
        Navigator.of(context).push(
          MaterialPageRoute<void>(
            builder: (_) => DeviceDetailScreen(
              initialDevice: device,
              accessToken: accessToken,
              deviceRepository: deviceRepository,
            ),
          ),
        );
      },
    );
  }
}

class _DeviceListMessage extends StatelessWidget {
  const _DeviceListMessage({
    required this.icon,
    required this.title,
    required this.message,
    this.action,
  });

  final IconData icon;
  final String title;
  final String message;
  final Widget? action;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 48, color: colorScheme.primary),
            const SizedBox(height: 16),
            Text(
              title,
              textAlign: TextAlign.center,
              style: textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: 8),
            Text(message, textAlign: TextAlign.center),
            if (action != null) ...[const SizedBox(height: 16), action!],
          ],
        ),
      ),
    );
  }
}

class StatusChip extends StatelessWidget {
  const StatusChip({super.key, required this.label, required this.icon});

  final String label;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return Chip(
      avatar: Icon(icon, size: 18),
      label: Text(label),
      visualDensity: VisualDensity.compact,
      materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
    );
  }
}
