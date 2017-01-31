#!/usr/bin/env groovy
stage("Windows") {
  node('windows') {
    checkout poll: false, changelog: false, scm: scm
    bat ("Protobuild.exe --upgrade-all")
    bat ('Protobuild.exe --automated-build')
    archiveArtifacts 'Installers/Windows/ProtobuildWebInstall.exe'
  }
}

stage("Mac") {
  node('mac') {
    checkout poll: false, changelog: false, scm: scm
    sh ("mono Protobuild.exe --upgrade-all")
    sh ("mono Protobuild.exe --automated-build")
    archiveArtifacts 'Installers/MacOS/install.sh'
  }
}

stage("Linux") {
  node('linux') {
    checkout poll: true, changelog: true, scm: scm
    sh ("mono Protobuild.exe --upgrade-all")
    sh ("mono Protobuild.exe --automated-build")
    archiveArtifacts 'Installers/Linux/install.sh'
  }
}