# osucket
* Yet another osu! memory output program using WebSocket 

# Usage
  
#### 1. [Download the latest Release](https://github.com/kr3st1k/osucket/releases/latest)
  * Unzip files anywhere

#### 2. Run osucket & osu!
  * Input your WebSocket address in overlay (`ws://localhost:13371`)

## Parameters Reference

| Parameter | Standard value | Type     | Description                |
| :-------- | :------------- | :------- | :------------------------- |
| `-timer (-delay)` | 500 |  `int` | Delay in sending data |
| `-port`      | 13371 |  `int` | WebSocket port |
| `-showerrors`      | false   | `bool?` | Output errors |

#### Parameters execution example
`osucket.exe -timer=500 -port=13371 -showerrors`

## Authors

- [@Kr3st1k](https://www.github.com/kr3st1k)

- [@ve3xone](https://www.github.com/ve3xone)

- [@oSumAtrIX](https://www.github.com/oSumAtrIX)

# Special Thanks to:

* [Piotrekol](https://github.com/Piotrekol/) and his [ProcessMemoryDataFinder](https://github.com/Piotrekol/ProcessMemoryDataFinder) for getting memory values
