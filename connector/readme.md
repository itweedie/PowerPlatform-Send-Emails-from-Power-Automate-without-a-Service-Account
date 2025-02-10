# Readme Connector Folder

This folder contains the connector exported from the Power Platform using `paconn`.

## Overview

This connector allows you to send emails using the Microsoft Graph API. It has been exported from the Power Platform and can be imported back using the `paconn` CLI tool.

## Installation

To install `paconn`, you need to have Python installed on your machine. You can install `paconn` using pip:

```sh
pip install paconn
```

## Usage

### Exporting a Connector

To export a connector from the Power Platform, use the following command:

```sh
paconn export --connector <connector-id> --environment <environment-id> --output <output-folder>
```

### Importing a Connector

To import a connector to the Power Platform, use the following command:

```sh
paconn import --connector <connector-id> --environment <environment-id> --api-props <api-properties-file> --api-def <api-definition-file> --script <script-file>
```

## More Details

For more details on how to use `paconn`, please refer to the official documentation: [Custom Connectors using paconn CLI](https://learn.microsoft.com/en-us/connectors/custom-connectors/paconn-cli)