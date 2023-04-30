variable "aws_region" {
  default = "eu-west-2"
}

variable "donate_craft_version" {
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