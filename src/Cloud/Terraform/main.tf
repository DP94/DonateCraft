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