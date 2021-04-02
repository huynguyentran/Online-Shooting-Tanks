﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TankWars;

namespace Controller
{
    [JsonObject(MemberSerialization.OptIn)]
    class ControlCommands
    {
        [JsonProperty(PropertyName = "moving")]
        private string movement;

        [JsonIgnore]
        public string Move
        {
            get
            {
                return movement;
            }
            set
            {
                movement = value;
            }
        }

        [JsonProperty(PropertyName = "fire")]
        private string fireType;

        [JsonIgnore]
        public string Fire
        {
            get
            {
                return fireType;
            }
            set
            {
                fireType = value;
            }
        }


        [JsonProperty(PropertyName = "tdir")]
        private Vector2D direction;

        [JsonIgnore]
        public Vector2D directionOfTank
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }


        public static string Serialize(ControlCommands command)
        {
            return JsonConvert.SerializeObject(command);
        }

    }




}