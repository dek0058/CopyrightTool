# CopyrightTool&nbsp;[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) ![C#](https://img.shields.io/badge/Language-C%23-239120?style=flat&logo=c-sharp&logoColor=white) ![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet&logoColor=white)

소스코드의 저작권 표기를 자동화 하기 위해 만든 도구 입니다.


## 🛠️ 사용법

본 도구는 **명령줄 인수(Command-line arguments)**를 통해 실행됩니다.

### 실행 구문

Usage: --init: Create config file in running path. --run [config file path]: Run program

### 상세 설명

* **`--init`**
    * **기능:** 도구 실행 경로에 **설정 파일(Config file)**을 생성합니다. 이 파일을 수정하여 저작권 문구, 제외할 경로, 파일 확장자 등을 지정할 수 있습니다.
    * **예시:** `CopyrightTool.exe --init`

* **`--run [설정 파일 경로]`**
    * **기능:** 지정된 설정 파일을 읽어 들여 저작권 표기 자동화 작업을 실행합니다.
    * **예시:** `CopyrightTool.exe --run config.yaml`


## ⚙️ 설정 파일 구조

CopyrightTool은 YAML 형식의 설정 파일을 사용하며, `--run` 인수로 파일 경로를 지정해야 합니다.

### 파일 내용

```yaml
copyright: Person
rootPath: {Path}
excludePaths: []
fileExtensions:
- .cs
- .cpp
- .h
- .c
- .go
- .hpp
```
