pipeline {
	agent any
	
	environment {
		BuildConfiguration="Debug"
	}

	stages {
		stage('Build') {
			steps {
				sh "dotnet build -c $BuildConfiguration"
			}
		}
		
		stage('Pack & Push Tgs.SerialNumberGenerator') {
			steps {
				sh 'dotnet pack Tgs.SerialNumberGenerator/Tgs.SerialNumberGenerator.csproj -c $BuildConfiguration --no-build --no-restore'
				sh 'dotnet nuget push Tgs.SerialNumberGenerator/bin/$BuildConfiguration/*.nupkg -s "raypro-baget" -k admin --skip-duplicate'
			}
		}
	}

	post {
        always {
            step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: false, keepLongStdio: true])
        }
    }
}
