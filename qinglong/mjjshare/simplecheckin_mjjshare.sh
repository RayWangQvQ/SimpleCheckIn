#!/usr/bin/env bash
# new Env("mjjshare签到")
# cron 5 0 * * * simplecheckin_mjjshare.sh
. simplecheckin_base.sh

cd ./src/SimpleCheckIn.MjjShare

export SimpleCheckIn_MjjShare_Run=CheckIn && \
export SimpleCheckIn_MjjShare_SystemConfig__Platform=qinglong && \
dotnet run --configuration Release
