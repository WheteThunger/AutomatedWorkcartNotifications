## Features

- Allows notifying players via chat when automated workcarts collide with nearby triggers (e.g., when they stop nearby)
- Allows delaying notifications with a timer to support multiple use cases (e.g., arrival, departure)
- Allows configuring the chat notification icon
- Allows configuring multiple unique chat notifications
- Allows configuring the distance at which players will be notified per chat notification
- Skips chat notifications for players who are already on a workcart

## Required plugins

- [Automated Workcarts](https://umod.org/plugins/automated-workcarts)

## How it works

Note: This plugin assumes you are already familiar with the basic features of the Automated Workcarts plugin, particularly triggers. If you are not, please go learn about it before reading further.

The Automated Workcarts plugin allows you to assign custom commands to each workcart trigger, which will be executed when any Automated Workcart collides with that trigger. This plugin provides the `workcart.notify` command which is designed to be ran by workcart triggers. It can be used to notify players via chat when workcarts arrive at or depart from stops.

## Example usage

1. Go to a workcart trigger where you would like to add a notification.
2. Aim at the trigger and run the command `awt.addcommand workcart.notify $id Arrived 5`.
3. Aim at the trigger and run the command `awt.addcommand workcart.notify $id DepartingSoon 15`.

When an automated workcarts collides with that trigger, it will:

- Send a notification after 5 seconds to players within 40m of the workcart, letting them know that a workcart has arrived at the station.
- Send a notification after 15 seconds to players within 20m of the workcart, letting them know that the workcart is going to depart soon.

## Server commands

- `workcart.notify <workcart_id> <notification_name> <delay_seconds>` -- This command is the primary offering of this plugin. When executed, it waits the specified number of seconds, then sends the specified chat notification to players within a configurable distance.
  - `<workcard_id>` -- This refers to the Net ID of the workcart. The command uses the workcart's location to determine which players are nearby. When adding the `workcart.notify` command to a workcart trigger, always use the `$id` placeholder, which the Automated Workcarts plugin will automatically replace with the workcart's Net ID.
  - `<notification_name>` -- This determines which chat notification will be sent to nearby players. This must refer to an entry in the `Chat notifications` section of the configuration.
  - `<delay_seconds>` -- This determines how long to delay the chat notification after the command gets run. Delaying the notification is useful for situations where you want to synchronize the notification with when the workcart finally stops.

## Configuration

Default configuration:

```json
{
  "Chat SteamID icon": 0,
  "Chat notifications": {
    "Arrived": {
      "Max distance": 40.0
    },
    "DepartingSoon": {
      "Max distance": 20.0,
      "Max speed": 1.0
    }
  }
}
```

- `Chat SteamID icon` -- Set this to the Steam ID of the account whose avatar you would like to show next to chat notifications.
- `Chat notifications` -- List of possible chat notifications that can be called by `workcart.notify`. The chat message for each notification can be configured in the plugin's localization file. You can add as many notifications as you want. Each notification has the following options.
  - `Max distance` -- Only players within this distance from the workcart will be notified.
  - `Max speed` -- Notifications will be skipped if the workcart is going faster than this speed. Skipping notifications is useful in situations where the workcart departed early due to congestion.

## Localization

```json
{
  "Arrived": "All Aboard! A train has has arrived at the station.",
  "DepartingSoon": "All Aboard! The train will be departing soon."
}
```
