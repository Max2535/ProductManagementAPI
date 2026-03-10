pipeline {
    // ใช้ agent any เพื่อรัน pipeline บน Jenkins node โดยตรง
    // แต่ละ stage ที่ต้องใช้ .NET จะเรียก docker แยก พร้อม reuseNode true
    agent any

    environment {
        // NuGet cache - เก็บไว้ใน Jenkins home volume เพื่อใช้ร่วมกันระหว่าง containers
        NUGET_PACKAGES = '/var/jenkins_home/.nuget/packages'
        // Discord Webhook - เก็บใน Jenkins > Credentials > Secret text > ID: discord-webhook
        DISCORD_WEBHOOK = credentials('discord-webhook')
    }

    stages {
        stage('Checkout') {
            steps {
                git url: 'https://github.com/Max2535/ProductManagementAPI.git',
                    branch: 'main',
                    credentialsId: 'github-auth'
            }
        }

        stage('Restore') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
                    reuseNode true  // ใช้ workspace เดิม ไม่สร้าง @2
                }
            }
            steps {
                echo 'Restoring NuGet packages...'
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
                    reuseNode true
                }
            }
            steps {
                echo 'Building the project...'
                sh 'dotnet build --configuration Release --no-restore'
            }
        }

        stage('Test') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
                    reuseNode true
                }
            }
            steps {
                echo 'Running unit tests...'
                sh 'dotnet test --configuration Release --no-build --verbosity normal'
            }
        }

        stage('Publish') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:9.0'
                    reuseNode true
                }
            }
            steps {
                echo 'Publishing the application...'
                sh 'dotnet publish --configuration Release --no-build -o ./publish'
            }
        }

        stage('Docker Build & Deploy') {
            steps {
                echo 'Preparing environment and Deploying...'
                // สร้างไฟล์ .env ชั่วคราวเพื่อให้ Docker Compose มีค่ารหัสผ่านใช้งาน
                sh """
                echo "SA_PASSWORD=YourStr0ng!Pass" > .env
                echo "MSSQL_PID=Developer" >> .env
                echo "SQLSERVER_PORT=1433" >> .env
                echo "API_HTTP_PORT=5000" >> .env
                echo "API_HTTPS_PORT=5001" >> .env
                echo "ASPNETCORE_ENVIRONMENT=Development" >> .env
                echo "DB_CONNECTION_STRING=Server=sqlserver;Database=ProductDb;User Id=sa;Password=YourStr0ng!Pass;TrustServerCertificate=True;" >> .env
                echo "RABBITMQ_HOSTNAME=rabbitmq" >> .env
                echo "RABBITMQ_DEFAULT_USER=admin" >> .env
                echo "RABBITMQ_DEFAULT_PASS=rabbitmq123" >> .env
                echo "RABBITMQ_PORT=5672" >> .env
                echo "RABBITMQ_MANAGEMENT_PORT=15672" >> .env
                echo "NGINX_HTTP_PORT=80" >> .env
                echo "NGINX_HTTPS_PORT=443" >> .env
                """

                // ลบ containers เก่าทั้งหมด (ป้องกัน name conflict และ port conflict)
                sh 'docker compose down --remove-orphans || true'
                sh '''
                    docker rm -f productmanagement-sqlserver \
                                 productmanagement-rabbitmq \
                                 productmanagement-api \
                                 productmanagement-nginx 2>/dev/null || true
                '''
                // หา containers ที่ยังครอง port อยู่ แล้วลบออก
                sh '''
                    for port in 1433 5672 15672 5000 5001 80 443; do
                        cid=$(docker ps -q --filter "publish=${port}")
                        if [ -n "$cid" ]; then
                            docker rm -f $cid || true
                        fi
                    done
                '''
                // สร้าง custom Nginx image แทนการ bind mount
                // เพื่อหลีกเลี่ยงปัญหา directory/file conflict บน host
                sh '''
                    rm -rf nginx/nginx.conf
                    mkdir -p nginx

                    cat > nginx/nginx.conf << 'NGINXEOF'
events { worker_connections 1024; }

http {
    upstream api {
        server productmanagement-api:8080;
    }

    server {
        listen 80;

        location / {
            proxy_pass         http://api;
            proxy_http_version 1.1;
            proxy_set_header   Upgrade $http_upgrade;
            proxy_set_header   Connection keep-alive;
            proxy_set_header   Host $host;
            proxy_cache_bypass $http_upgrade;
        }
    }
}
NGINXEOF

                    cat > nginx/Dockerfile << 'DOCKEREOF'
FROM nginx:alpine
COPY nginx.conf /etc/nginx/nginx.conf
DOCKEREOF

                    cat > docker-compose.override.yml << 'OVERRIDEEOF'
services:
  nginx:
    build:
      context: ./nginx
      dockerfile: Dockerfile
    image: productmanagement-nginx:latest
    volumes: []
OVERRIDEEOF
                '''
                sh 'docker compose build'
                sh 'docker compose up -d'
            }
        }
    }

    post {
        success {
            echo 'Build, Test และ Publish สำเร็จแล้ว!'
            archiveArtifacts artifacts: 'publish/**', fingerprint: true

            // แจ้งเตือน Discord เมื่อสำเร็จ (แถบสีเขียว)
            sh """
            curl -H "Content-Type: application/json" \\
                 -X POST \\
                 -d '{
                      "username": "Jenkins Bot",
                      "avatar_url": "https://jenkins.io/images/logos/jenkins/jenkins.png",
                      "embeds": [{
                        "title": "✅ Build Success!",
                        "description": "**Project:** ${env.JOB_NAME}\\n**Build:** #${env.BUILD_NUMBER}\\n**Status:** SUCCESS\\n\\n[ดูรายละเอียดบน Jenkins](${env.BUILD_URL})",
                        "color": 3066993
                      }]
                    }' \\
                 ${DISCORD_WEBHOOK}
            """
        }
        failure {
            echo 'การ Build ล้มเหลว กรุณาตรวจสอบ Console Output อีกครั้ง'

            // แจ้งเตือน Discord เมื่อพัง (แถบสีแดง)
            sh """
            curl -H "Content-Type: application/json" \\
                 -X POST \\
                 -d '{
                      "username": "Jenkins Bot",
                      "avatar_url": "https://jenkins.io/images/logos/jenkins/jenkins.png",
                      "embeds": [{
                        "title": "❌ Build Failed!",
                        "description": "**Project:** ${env.JOB_NAME}\\n**Build:** #${env.BUILD_NUMBER}\\n**Status:** FAILURE\\n\\n[ตรวจสอบสาเหตุที่นี่](${env.BUILD_URL}console)",
                        "color": 15158332
                      }]
                    }' \\
                 ${DISCORD_WEBHOOK}
            """
        }
    }
}
