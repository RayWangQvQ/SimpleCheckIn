#!/usr/bin/env bash
# new Env("ikuuu签到")
# cron 1 0 * * * simplecheckin_ikuuu.sh
. simplecheckin_base.sh

cd ./src/SimpleCheckIn.Ikuuu

export SimpleCheckIn_Ikuuu_Run=CheckIn && \
export SimpleCheckIn_Ikuuu_IkuuuConfig__Platform=qinglong && \
dotnet run --configuration Release
