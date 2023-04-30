terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "4.52.0"
    }
  }
  required_version = ">= 1.1.0"

  backend "s3" {
    bucket = "donatecraft-terraform"
    key    = "donatecraft.tfstate"
    region = "eu-west-2"
  }
}

provider "aws" {
  region = var.aws_region
}

resource "aws_dynamodb_table" "players" {
  name           = "Player${var.donate_craft_version}"
  billing_mode   = "PROVISIONED"
  read_capacity  = 20
  write_capacity = 20
  hash_key       = "id"

  attribute {
    name = "id"
    type = "S"
  }
}