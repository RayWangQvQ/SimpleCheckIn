#!/usr/bin/env bash
# new Env("mjjshare签到")
# cron 5 0 * * * simplecheckin_mjjshare.sh
. auto_task_base.sh

cd ./src/SimpleCheckIn.MjjShare

export SimpleCheckIn.MjjShare_Run=Hello && \
export SimpleCheckIn.MjjShare_SystemConfig__Platform=qinglong && \
dotnet run --configuration Release