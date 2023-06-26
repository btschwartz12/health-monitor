// Decides which configuration to build by git branch name
// eg. master -> Release, dev -> Debug, etc.
def getConfigurationByBranch() {
	switch (GIT_BRANCH) {
		case ["master"]:
			return "Release"
		default:
			return "Debug"
	}
}

// Decides which Octopus Deploy Channel to create
// a release for by git branch name
def getDeploymentChannelByBranch() {
	switch (GIT_BRANCH) {
		//case ["master"]:
		//	return "Production" :)
		default:
			return "Development"
	}
}

pipeline {
	agent any
	environment { 
		// Project Specific 
		SOLUTION_PATH = './ISHealthMonitor.sln'
		PROJECT_PATH = './ISHealthMonitor/ISHealthMonitor.UI.csproj'
		OCTOPUSDEPLOY_PROJECT = '"IS Health Monitor"'
		PACKAGE_NAME = "ISHealthMonitor"
		OCTOPUSDEPLOY_CHANNEL = getDeploymentChannelByBranch()
		BUILD_CONFIGURATION = getConfigurationByBranch()
        ARTIFACTS_FOLDER = './artifacts'
		PUBLISH_FOLDER = './publish'
		FRAMEWORK = 'net6.0'
		MINOR_VERSION = 0
		MAJOR_VERSION = 1

		// Tools (Paths on Jenkins Server)
	
		GITVERSION = '"C:/ProgramData/chocolatey/bin/GitVersion.exe"'

		DOTNET = '"C:/Program Files/dotnet/dotnet.exe"'

		// Deployment
		OCTOPUSDEPLOY_URL = 'https://octopus.hyland.com'
		OCTOPUSDEPLOY_FEED = "${OCTOPUSDEPLOY_URL}/nuget/packages"
		OCTOPUSDEPLOY_APIKEY = credentials('octopusdeploy_api_key')
		VERSION_TAG = "${BUILD_CONFIGURATION == 'Debug' ? '-dev' : ''}"
	}
	stages {
		// MSBuild
		// Clean, NuGet Restore, and Build
		stage('Build') {
			steps {
				bat "${DOTNET} clean ${SOLUTION_PATH} -c ${BUILD_CONFIGURATION}"
				bat "${DOTNET} build ${PROJECT_PATH} -c ${BUILD_CONFIGURATION}"
			}
		}
		// Git Version
		stage('Version') {
			environment {
                IGNORE_NORMALISATION_GIT_HEAD_MOVE = "1"
            }
			steps {
				script {
					env.BUILD = (bat(script: "@echo off && ${GITVERSION} /showvariable CommitsSinceVersionSource", returnStdout: true)).trim()
					env.VERSION = "${MAJOR_VERSION}.${MINOR_VERSION}.${env.BUILD}${env.VERSION_TAG ?: ''}"
				}
			}
		}
		// Create NuGet Package and Push to the Octopus Deploy Feed
		// note: only pushes a package for master or dev, not feature branches
		// octopack must also be installed as a nuget package in the project(s) 
		// that should be deployed
		stage('Package') {
			when {
				anyOf {
					branch 'master'
					branch 'dev'
				}
			}
			steps {
                bat "${DOTNET} publish ${PROJECT_PATH} -c ${BUILD_CONFIGURATION} -o ${PUBLISH_FOLDER}/${PACKAGE_NAME} -f ${FRAMEWORK}"
				bat "${DOTNET} octo pack --id=${PACKAGE_NAME} --version=${VERSION} --outFolder=${ARTIFACTS_FOLDER} --basePath=${PUBLISH_FOLDER}/${PACKAGE_NAME} --format zip"
				bat "${DOTNET} octo push --package=${ARTIFACTS_FOLDER}/${PACKAGE_NAME}.${VERSION}.zip --server=${OCTOPUSDEPLOY_URL} --apiKey ${OCTOPUSDEPLOY_APIKEY}"
			}
		}
		// Create a release in Octopus Deploy
		stage('Deployment Channel') {
			when {
				anyOf {
					branch 'master'
					branch 'dev'
				}
			}
			steps {
                bat "${DOTNET} octo create-release --project=${OCTOPUSDEPLOY_PROJECT} --releaseNumber=${VERSION} --packageVersion=${VERSION} --channel=${OCTOPUSDEPLOY_CHANNEL} --server=${OCTOPUSDEPLOY_URL} --apiKey=${OCTOPUSDEPLOY_APIKEY}"
			}
		}
	}
	post {
        cleanup {
            cleanWs()
        }
    }
}