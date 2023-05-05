variable "aws_region" {
  default = "eu-west-2"
}

variable "donate_craft_version" {
  description = "The version of the DonateCraft API - version used in all resources such as Lambda etc. E.g. pr-, prod-, dev-, qa-"
}

variable "donate_craft_environment" {
  description = "The environment DonateCraft is running on - used for DDB tables to keep them consistent across PR builds"
}

variable "donate_craft_ui" {
  description = "The UI URL for DonateCraft"
  default = "http://donatecraftui.s3-website.eu-west-2.amazonaws.com"
}

variable "just_giving_api_url" {
  description = "The Url for JustGiving API"
  default = "https://api.staging.justgiving.com/"
}

variable "just_giving_api_key" {
  description = "API key for the JustGiving API"
}