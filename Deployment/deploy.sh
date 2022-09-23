#!/bin/sh
# sudo DEPLOY_ENV=qa bash deploy.sh
if [[ -z "${DEPLOY_ENV}" ]]; then
    DEPLOY_ENV="dev"
    echo "You have not specified DEPLOY_ENV environment variable, defaulting to '${DEPLOY_ENV}' environment"
fi
if [[ -z "${DISCORD_WEBHOOK}" ]]; then
    echo "You need to supply a discord webhook via DISCORD_WEBHOOK environment variable in order to notify about deployment"
    exit 1
fi

generate_successful_response() {
  cat <<EOF
{
  "embeds": [{
    "title": "QA Instance update",
    "description": "Successfully deployed a new QA instance at $(date)",
    "color": "5793266"
  }]
}
EOF
}

sudo systemctl stop scriptycord-${DEPLOY_ENV}
sudo systemctl disable --now scriptycord-${DEPLOY_ENV}

# Setup database container
db_name="scriptycord-db"
if [ ! "$(docker ps -q -f name=${db_name})" ]; then
    if [ "$(docker ps -aq -f status=exited -f name=${db_name})" ]; then
        docker start $db_name
    else
        declare -A connection_values=(['port']='0' ['Password']='')
        connection_string="$(jq .ConnectionStrings.DefaultConnection ../ScriptyCord.Migrations/appsettings.${DEPLOY_ENV}.json)"
        IFS=';' connection_tokens=( $connection_string )
        for pair in "${connection_tokens[@]}";
        do
            IFS='=' pair_tokens=( $pair )
            connection_values[${pair_tokens[0]}]="${pair_tokens[1]}"
        done

        echo "${connection_values[Password]}"
        echo "docker run -it --name $db_name -p ${connection_values[port]}:${connection_values[port]} -e POSTGRES_PASSWORD="${connection_values[Password]}" -d postgres:latest"
        docker run -it --name $db_name -p ${connection_values[port]}:${connection_values[port]} -e POSTGRES_PASSWORD="${connection_values[Password]}" -d postgres:latest
    
        echo "Waiting 15 seconds for database to go up..."
        sleep 15s
    fi
fi

# Migrations
echo "Building migrator"
dotnet build ../ScriptyCord.Migrations/ --os linux --configuration release --output ./Builds/ScriptyCord.Migrations/
echo "Running migrations"
cd Builds/ScriptyCord.Migrations/ && ENVIRONMENT_TYPE=$DEPLOY_ENV ./ScriptyCord.Migrations && cd ../../

# Deploy the bot
echo "Deploying the bot"
dotnet publish ../ScriptyCord.Bot/ --os linux --configuration release --output ./Builds/ScriptyCord.Bot/
mkdir ./Builds/ScriptyCord.Bot/Downloads
mkdir ./Builds/ScriptyCord.Bot/Downloads/Audio

# Setup systemd service
sudo cp scriptycord-${DEPLOY_ENV}.service /etc/systemd/system/scriptycord-${DEPLOY_ENV}.service
sudo systemctl enable --now scriptycord-${DEPLOY_ENV}
sudo systemctl start scriptycord-${DEPLOY_ENV}

curl -H "Content-Type: application/json" -X POST -d "$(generate_successful_response)" $DISCORD_WEBHOOK