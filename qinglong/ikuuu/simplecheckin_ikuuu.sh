#!/usr/bin/env bash
# new Env("ikuuu签到")
# cron 1 0 * * * simplecheckin_ikuuu.sh
. auto_task_base.sh

cd ./src/SimpleCheckIn.Ikuuu

export SimpleCheckIn_Ikuuu_Run=Hello && \
export SimpleCheckIn_Ikuuu_IkuuuConfig__Platform=qinglong && \
dotnet run --configuration Release
