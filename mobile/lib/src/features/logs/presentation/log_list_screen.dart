import 'package:flutter/material.dart';

import '../../devices/presentation/device_list_screen.dart';

class LogListScreen extends StatelessWidget {
  const LogListScreen({super.key, this.device});

  final DeviceSummary? device;

  static const _logs = [
    _LogEntry(
      processName: 'steam.exe',
      action: 'blocked',
      mode: 'study',
      message: 'Blocked distracting application during study mode.',
      createdAt: '10:24',
    ),
    _LogEntry(
      processName: 'chrome.exe',
      action: 'opened',
      mode: 'study',
      message: 'Allowed browser activity.',
      createdAt: '10:18',
    ),
    _LogEntry(
      processName: 'discord.exe',
      action: 'blocked',
      mode: 'punishment',
      message: 'Blocked chat application during strict mode.',
      createdAt: '09:42',
    ),
  ];

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final subtitle = device == null
        ? 'Review recent child device activity.'
        : 'Recent activity for ${device!.name}.';

    return Scaffold(
      appBar: AppBar(title: const Text('Logs')),
      body: SafeArea(
        child: ListView.separated(
          padding: const EdgeInsets.all(20),
          itemCount: _logs.length + 1,
          separatorBuilder: (_, index) =>
              SizedBox(height: index == 0 ? 16 : 10),
          itemBuilder: (context, index) {
            if (index == 0) {
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Activity Logs',
                    style: textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    subtitle,
                    style: textTheme.bodyMedium?.copyWith(
                      color: Theme.of(context).colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              );
            }

            return _LogTile(log: _logs[index - 1]);
          },
        ),
      ),
    );
  }
}

class _LogTile extends StatelessWidget {
  const _LogTile({required this.log});

  final _LogEntry log;

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
        backgroundColor: colorScheme.secondaryContainer,
        foregroundColor: colorScheme.onSecondaryContainer,
        child: Icon(
          log.action == 'blocked'
              ? Icons.block_outlined
              : Icons.open_in_new_outlined,
        ),
      ),
      title: Text(log.processName),
      subtitle: Padding(
        padding: const EdgeInsets.only(top: 6),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(log.message),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                Chip(
                  avatar: const Icon(Icons.rule_outlined, size: 18),
                  label: Text(log.action),
                  visualDensity: VisualDensity.compact,
                  materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
                Chip(
                  avatar: const Icon(Icons.shield_outlined, size: 18),
                  label: Text(log.mode),
                  visualDensity: VisualDensity.compact,
                  materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
              ],
            ),
          ],
        ),
      ),
      trailing: Text(log.createdAt),
    );
  }
}

class _LogEntry {
  const _LogEntry({
    required this.processName,
    required this.action,
    required this.mode,
    required this.message,
    required this.createdAt,
  });

  final String processName;
  final String action;
  final String mode;
  final String message;
  final String createdAt;
}
