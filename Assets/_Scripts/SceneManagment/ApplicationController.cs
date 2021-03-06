﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MainGame;
using System;

namespace Controller
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] SceneLoader loader;

        public static ApplicationController app { get; private set; }

        void Awake()
        {
            app = this;
        }

        void Start()
        {
           LoadStartup();
        }

        //public void Level1()
        //{
        //    loader.Load("level_1", () => Helpers.Log("Level 1 loaded"));
        //}

        //public void Level2()
        //{
        //    loader.Load("level_2", () => Helpers.Log("Level 2 loaded"));
        //}

        //public void Level3()
        //{
        //    loader.Load("level_3", () => Helpers.Log("Level 3 laoded"));
        //}

        public void LoadLevel(string name)
        {
            loader.Load(name, () => Helpers.Log(name + " laoded"));
        }
        public void ToStartup()
        {
            loader.UnloadAll(LoadStartup);
        }

        void LoadStartup()
        {
            loader.Load("startup");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
