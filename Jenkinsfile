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

// Decides which configuration to build by git branch name
def getEnvironmentNameByBranch() {
	switch (GIT_BRANCH) {
		case ["master"]:
			// Extra spacing here is a work around for Groovy treating variables consisting of just a blank string as not set
			return " "
		default:
			return "/p:EnvironmentName=Development"
	}
}

// Decides which Octopus Deploy Channel to create
// a release for by git branch name
def getDeploymentChannelByBranch() {
	switch (GIT_BRANCH) {
		case ["master"]:
			return "Production"
		case ["dev"]:
			return "Development"
		default:
			return "Default"
	}
}

pipeline {
	agent any
	environment {
		// Project Specific 
		SOLUTION_PATH = './ISHealthMonitor.sln'
		OCTOPUSDEPLOY_PROJECT = '"IS Health Monitor"'
		PACKAGE_NAME = "ISHealthMonitor.Web"
		OCTOPUSDEPLOY_CHANNEL = getDeploymentChannelByBranch()
		BUILD_CONFIGURATION = getConfigurationByBranch()
		ENVIRONMENT_NAME = getEnvironmentNameByBranch()
		ARTIFACTS_FOLDER = './artifacts'
		BUILD_FOLDER = './build'

		// Tools (Paths on Jenkins Server)
		DOTNET = '"C:/Program Files/dotnet/dotnet.exe"'
		GITVERSION = '"C:/ProgramData/chocolatey/bin/GitVersion.exe"'

		// Deployment
		OCTOPUSDEPLOY_URL = 'https://octopus.hyland.com'
		OCTOPUSDEPLOY_APIKEY = credentials('octopusdeploy_api_key')
	}
	stages {
		// Perform the initial build
		stage('Build') {
			steps {
				bat "${DOTNET} clean ${SOLUTION_PATH}"
				bat "${DOTNET} restore ${SOLUTION_PATH}"
				bat "${DOTNET} build ${SOLUTION_PATH} -c ${BUILD_CONFIGURATION}"
			}
		}
		// SemVer
		stage('Version') {
			steps {
				script {
					env.VERSION = (bat(script: "@echo off && ${GITVERSION} /showvariable FullSemVer", returnStdout: true)).trim()
				}
			}
		}
		// Run the test suite
		stage('Test') {
			steps {
				bat "${DOTNET} test ${SOLUTION_PATH} -c ${BUILD_CONFIGURATION} --logger \"trx;LogFileName=TestResults.${VERSION}.trx\" --no-build"
			}
		}
		// Package up a release
		// publishes to a local build folder and creates an artifact
		stage('Package') {
			when {
				anyOf {
					branch 'master'
					branch 'dev'
				}
			}
			steps {
				bat "${DOTNET} publish ${SOLUTION_PATH} -c ${BUILD_CONFIGURATION} -o ${BUILD_FOLDER}/${PACKAGE_NAME} ${ENVIRONMENT_NAME}"
				bat "${DOTNET} octo pack --id=${PACKAGE_NAME} --version=${VERSION} --outFolder=${ARTIFACTS_FOLDER} --basePath=${BUILD_FOLDER}/${PACKAGE_NAME}"
				bat "${DOTNET} octo push --package=${ARTIFACTS_FOLDER}/${PACKAGE_NAME}.${VERSION}.nupkg --server=${OCTOPUSDEPLOY_URL} --apiKey ${OCTOPUSDEPLOY_APIKEY}"
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
				bat "${DOTNET} octo create-release -project=${OCTOPUSDEPLOY_PROJECT} --releaseNumber=${VERSION} --channel=${OCTOPUSDEPLOY_CHANNEL} --server=${OCTOPUSDEPLOY_URL} --apiKey=${OCTOPUSDEPLOY_APIKEY}"
			}
		}
	}
	post {
        cleanup {
            cleanWs()
        }
    }
}