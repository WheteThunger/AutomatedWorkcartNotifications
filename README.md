## Features

- Allows notifying players via chat when automated trains collide with nearby triggers (e.g., when they stop nearby)
- Allows playing the train horn
- Allows delaying notifications with a timer to support multiple use cases (e.g., arrival, departure)
- Allows configuring the chat notification icon
- Allows configuring multiple unique chat notifications
- Allows configuring the distance at which players will be notified per chat notification
- Skips chat notifications for players who are already on a train

## Required plugins

- [Automated Workcarts](https://umod.org/plugins/automated-workcarts)

## How it works

Note: This plugin assumes you are already familiar with the basic features of the Automated Workcarts plugin, particularly triggers. If you are not, please go learn about it before reading further.

The Automated Workcarts plugin allows you to assign custom commands to each trigger, which will be executed when any automated train collides with that trigger. This plugin provides the `workcart.notify` command which is designed to be ran by triggers. It can be used to notify players when trains arrive at or depart from stops.

## Example usage

1. Go to a trigger where you would like to add a notification.
2. Aim at the trigger and run the command `awt.addcommand workcart.notify $id Arrived 5`.
3. Aim at the trigger and run the command `awt.addcommand workcart.notify $id DepartingSoon 15`.

When an automated train collides with that trigger, it will:

- Send a notification after 5 seconds to players within 40m of the train, letting them know that a train has arrived at the station.
- Send a notification after 15 seconds to players within 20m of the train, letting them know that the train is going to depart soon.

## Server commands

- `workcart.notify <train_engine_id> <notification_name> <delay_seconds>` -- This command is the primary offering of this plugin. When executed, it waits the specified number of seconds, then sends the specified chat notification to players within a configurable distance.
  - `<workcard_id>` -- This refers to the Net ID of the train. The command uses the train's location to determine which players are nearby. When adding the `workcart.notify` command to a trigger, always use the `$id` placeholder, which the Automated Workcarts plugin will automatically replace with the train's Net ID.
  - `<notification_name>` -- This determines which chat notification will be sent to nearby players. This must refer to an entry in the `Notifications` section of the configuration.
  - `<delay_seconds>` -- This determines how long to delay the chat notification after the command gets run. Delaying the notification is useful for situations where you want to synchronize the notification with when the train finally stops.

## Configuration

Default configuration:

```json
{
  "Chat SteamID icon": 0,
  "Notifications": {
    "Arrived": {
      "Broadcast chat message": true,
      "Horn duration (seconds)": 0.0,
      "Max distance": 40.0
    },
    "DepartingSoon": {
      "Broadcast chat message": true,
      "Horn duration (seconds)": 0.0,
      "Max distance": 20.0,
      "Max speed": 1.0
    }
  }
}
```

- `Chat SteamID icon` -- Set this to the Steam ID of the account whose avatar you would like to show next to chat notifications.
- `Notifications` -- This section allows you to define notifications that can be called by `workcart.notify`. You can add as many notifications as you want. Each notification has the following options.
  - `Broadcast chat message` (`true` or `false`) -- While `true`, nearby players will receive a chat notification. The chat message can be configured in the plugin's localization file.
  - `Horn duration (seconds)` -- While greater than `0.0`, the train will play its horn for that many seconds.
  - `Max distance` -- While the chat notification is enabled, only players within this radius from the train will receive the chat notification.
  - `Max speed` -- Notifications will be skipped if the train is going faster than this speed. Skipping notifications is useful in situations where the train departed early due to congestion.

## Localization

```json
{
  "Arrived": "All Aboard! A train has has arrived at the station.",
  "DepartingSoon": "All Aboard! The train will be departing soon."
}
```
