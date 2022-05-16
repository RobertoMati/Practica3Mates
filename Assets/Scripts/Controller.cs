using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    List<int> ListRobber = new List<int>(); //lista para las casillas del ladrón
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

//OK        //TODO: Inicializar matriz a 0's
        //NumTiles es el número de casillas que hay en el tablero (64)
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriu[i, j] = 0;
            }
        }

//OK        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            if (i % Constants.TilesPerRow != 0)
            {
                matriu[i, i - 1] = 1;
            }
            if (i % Constants.TilesPerRow != Constants.TilesPerRow - 1)
            {
                matriu[i, i + 1] = 1;
            }
            if (i < Constants.TilesPerRow * (Constants.TilesPerRow - 1))
            {
                matriu[i, i + Constants.TilesPerRow] = 1;
            }
            if (i >= Constants.TilesPerRow)
            {
                matriu[i, i - Constants.TilesPerRow] = 1;
            }
        }

//OK        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                if (matriu[i, j] == 1)
                {
                    tiles[i].adjacency.Add(j);
                }
            }
        }

    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */
        robber.GetComponent<RobberMove>().MoveToTile(tiles[robber.GetComponent<RobberMove>().currentTile]);
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
        //variable para guardarnos el número en el que se encuentra la ficha         
        int indexcurrentTile;        

        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        //reiniciamos las variables
        tiles[indexcurrentTile].visited = true;  
        tiles[indexcurrentTile].distance = 0;
        tiles[indexcurrentTile].parent = null;

        nodes.Enqueue(tiles[indexcurrentTile]); //de lista de nodes ponemos en la cola la casilla en la que estas

        Tile antes = null; //creamos un objeto para el algoritmo BFS

        while (nodes.Count != 0)
        {
            antes = nodes.Dequeue(); //nos guardamos en la variable antes el elemento que quitamos de la cola 
            int antes2 = antes.numTile; //nos guardamos el numero de la variable

            foreach (int adyacent in tiles[antes2].adjacency) //recorremos los elemento adyacentes a el numero guardado
            {
                if (tiles[adyacent].visited == false) //si la primera casilla no esta visitada
                {
                    tiles[adyacent].visited = true;  //la ponemos a true
                    tiles[adyacent].distance = tiles[antes2].distance + 1; //a la distancia del adyacente le asignamos la del nodo en la que se encuentra más uno
                    tiles[adyacent].parent = tiles[antes2]; //la casilla por la cual llegamos le asignamos el valor de la variable antes 2
                    nodes.Enqueue(tiles[adyacent]); //añadimos el elemento a la cola de la lista de nodos

                    if (tiles[adyacent].distance <= 2) //comprobamos si la distancia es igual o menor que dos
                    {
                        if (cop == false) //si es un ladron
                        {
                            ListRobber.Add(tiles[adyacent].numTile);  //nos guaramos el elemento en la lista 
                        }
                        tiles[adyacent].selectable = true;  //si no la podemos seleccionar
                        if (cops[0].GetComponent<CopMove>().currentTile == tiles[adyacent].numTile || cops[1].GetComponent<CopMove>().currentTile == tiles[adyacent].numTile) //si es un policia 
                        {
                            tiles[adyacent].selectable = false; //no se puede seleccionar la casilla del policia
                        }
                    }

                }
            }

        }

    }
    
   
    

    

   

       
}
