import 'package:flutter/material.dart';

import 'device_detail_screen.dart';

class DeviceListScreen extends StatelessWidget {
  const DeviceListScreen({super.key});

  static const _devices = [
    DeviceSummary(
      name: 'Study Room PC',
      computerName: 'STUDY-PC',
      mode: 'study',
      isOnline: true,
      lastSeen: 'Online now',
    ),
    DeviceSummary(
      name: 'Gaming Laptop',
      computerName: 'GAME-LAPTOP',
      mode: 'fun',
      isOnline: false,
      lastSeen: 'Last seen 2 hours ago',
    ),
  ];

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;

    return Scaffold(
      appBar: AppBar(title: const Text('Devices')),
      body: SafeArea(
        child: ListView.separated(
          padding: const EdgeInsets.all(20),
          itemCount: _devices.length + 1,
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

            return _DeviceTile(device: _devices[index - 1]);
          },
        ),
      ),
    );
  }
}

class _DeviceTile extends StatelessWidget {
  const _DeviceTile({required this.device});

  final DeviceSummary device;

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
            builder: (_) => DeviceDetailScreen(device: device),
          ),
        );
      },
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

class DeviceSummary {
  const DeviceSummary({
    required this.name,
    required this.computerName,
    required this.mode,
    required this.isOnline,
    required this.lastSeen,
  });

  final String name;
  final String computerName;
  final String mode;
  final bool isOnline;
  final String lastSeen;
}
