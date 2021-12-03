import React from "react";
import { makeStyles, withStyles } from "@material-ui/core/styles";
import Drawer from "@material-ui/core/Drawer";
import AppBar from "@material-ui/core/AppBar";
import CssBaseline from "@material-ui/core/CssBaseline";
import Toolbar from "@material-ui/core/Toolbar";
import List from "@material-ui/core/List";
import Typography from "@material-ui/core/Typography";
import ListItem from "@material-ui/core/ListItem";
import ListItemText from "@material-ui/core/ListItemText";
import Grid from "@material-ui/core/Grid";
import Card from "@material-ui/core/Card";
import CardContent from "@material-ui/core/CardContent";
import CardMedia from "@material-ui/core/CardMedia";
import CardActionArea from "@material-ui/core/CardActionArea";
import ListItemSecondaryAction from "@material-ui/core/ListItemSecondaryAction";
import Checkbox from "@material-ui/core/Checkbox";
import CardActions from "@material-ui/core/CardActions";
import environments from "./resources/environments.json";
import Avatar from "@material-ui/core/Avatar";
import Divider from "@material-ui/core/Divider";
import Chip from "@material-ui/core/Chip";
import Button from "@material-ui/core/Button";

const drawerWidth = 170;

const BootstrapButton = withStyles({
  root: {
    boxShadow: "none",
    textTransform: "none",
    fontSize: 14,
    marginRight: "4px",
    marginTop: "5px",
    lineHeight: 1.5,
    backgroundColor: "#0069d9",
    fontFamily: [
      "-apple-system",
      "BlinkMacSystemFont",
      '"Segoe UI"',
      "Roboto",
      '"Helvetica Neue"',
      "Arial",
      "sans-serif",
      '"Apple Color Emoji"',
      '"Segoe UI Emoji"',
      '"Segoe UI Symbol"',
    ].join(","),
    "&:hover": {
      backgroundColor: "#0069d9",
      borderColor: "#0062cc",
      boxShadow: "none",
    },
    "&:active": {
      boxShadow: "none",
      backgroundColor: "#0062cc",
      borderColor: "#005cbf",
    },
    "&:focus": {
      boxShadow: "0 0 0 0.2rem rgba(0,123,255,.5)",
    },
  },
})(Button);

const useStyles = makeStyles((theme) => ({
  margin: {
    margin: theme.spacing(1),
  },
  root: {
    display: "flex",
    height: "270px",
    width: "220px",
  },
  appBar: {
    zIndex: theme.zIndex.drawer + 1,
  },
  drawer: {
    width: drawerWidth,
    flexShrink: 0,
  },
  drawerPaper: {
    width: drawerWidth,
    background: "#f5f5f5",
  },
  drawerContainer: {
    overflow: "auto",
  },
  content: {
    flexGrow: 1,
    padding: theme.spacing(3),
  },
  checkBox: {
    width: "100%",
    maxWidth: 360,
  },
}));

export default function Environments() {
  const classes = useStyles();
  const [checked, setChecked] = React.useState(["all"]);
  const distinctFilter = [];
  const handleChange = (event) => {
    let value = event.target.name;
    let newChecked = [...checked];
    if (event.target.checked) {
      if (value === "all") {
        newChecked = ["all"];
      } else {
        const allindex = newChecked.indexOf("all");
        if (allindex > -1) {
          newChecked.splice(allindex, 1);
        }
        newChecked.push(value);
      }
    } else {
      const currentindex = newChecked.indexOf(value);
      newChecked.splice(currentindex, 1);
      if (newChecked.length === 0) {
        newChecked = ["all"];
      }
    }
    setChecked(newChecked);
  };

  return (
    <div
      style={{
        display: "flex",
      }}
    >
      <CssBaseline />
      <AppBar position="fixed" className={classes.appBar}>
        <Toolbar>
          <Avatar alt="fission environments" src="./logo/fission-env.png" />
          <Typography
            variant="h5"
            noWrap
            style={{ paddingLeft: "5px", fontWeight: "bold" }}
          >
            Environments
          </Typography>
        </Toolbar>
      </AppBar>
      <Drawer
        className={classes.drawer}
        variant="permanent"
        classes={{
          paper: classes.drawerPaper,
        }}
      >
        <Toolbar />
        <Typography
          variant="subtitle1"
          noWrap
          style={{ padding: "10px", alignItems: "center" }}
        >
          Select environments
        </Typography>
        <Divider />
        <div className={classes.drawerContainer}>
          <List dense className={classes.checkBox}>
            <ListItem key="all" button>
              <ListItemText id="all" primary={`All`} />
              <ListItemSecondaryAction>
                <Checkbox
                  name="all"
                  edge="end"
                  onChange={handleChange}
                  checked={checked.indexOf("all") !== -1}
                  inputProps={{
                    "aria-labelledby": `checkbox-list-secondary-label-all`,
                  }}
                />
              </ListItemSecondaryAction>
            </ListItem>
            {environments.map((env) => {
              const value = env[0].name.split(" ")[0];
              const labelId = `checkbox-list-secondary-label-${value}`;
              if (distinctFilter.indexOf(value) > -1) return <></>;
              distinctFilter.push(value);
              return (
                <ListItem key={value} button>
                  <ListItemText id={labelId} primary={`${value}`} />
                  <ListItemSecondaryAction>
                    <Checkbox
                      name={value}
                      edge="end"
                      onChange={handleChange}
                      checked={checked.indexOf(value) !== -1}
                      inputProps={{ "aria-labelledby": labelId }}
                    />
                  </ListItemSecondaryAction>
                </ListItem>
              );
            })}
          </List>
        </div>
      </Drawer>
      <main className={classes.content}>
        <Toolbar />
        <Grid container spacing={4}>
          {environments.map(function (envs) {
            return envs.map((env, index) => {
              const value = env.name.split(" ")[0];
              //Show only checked cards
              if (checked.indexOf("all") > -1 || checked.indexOf(value) > -1) {
                return (
                  <Grid item key={index}>
                    <Card className={classes.root} variant="outlined">
                      <CardActionArea href={env.readme}>
                        <CardMedia
                          style={{
                            objectFit: "fill",
                            height: "100px",
                            paddingLeft: "5px",
                            paddingRight: "5px",
                            paddingTop: "5px",
                          }}
                          component="img"
                          alt="Contemplative Reptile"
                          height="140"
                          image={env.icon}
                          title="Contemplative Reptile"
                        />
                        <CardContent
                          style={{
                            paddingLeft: "15px",
                            paddingRight: "10px",
                            paddingTop: "3px",
                            paddingBottom: "0px",
                          }}
                        >
                          <Typography variant="body1" component="h2">
                            {env.name.length > 23
                              ? env.name.substring(0, 23) + "..."
                              : env.name}
                          </Typography>
                          <Typography
                            border="1"
                            variant="body2"
                            color="textSecondary"
                            component="p"
                            style={{ height: "58px" }}
                          >
                            {env.shortDescription.length > 84
                              ? env.shortDescription.substring(0, 84) + "..."
                              : env.shortDescription}
                          </Typography>
                        </CardContent>
                        <CardActions>
                          <div>
                            <div style={{ padding: "2px" }}>
                              {/* <Chip size="small" label={env.status} style={{marginRight:'5px',  background: '#2196f3' , color: 'white'}} /> */}
                              {/* <Chip size="small" label={env.runtimeVersion} style={{marginRight:'5px',background: "cadetblue" , color: 'white'}}/> */}
                              <Chip
                                size="small"
                                label={env.image + ":" + env.version}
                                style={{
                                  marginRight: "5px",
                                  background: "cadetblue",
                                  color: "white",
                                }}
                              />
                            </div>
                            <div></div>
                            <div>
                              <BootstrapButton
                                href={env.readme}
                                variant="contained"
                                size="small"
                                color="primary"
                              >
                                Learn more
                              </BootstrapButton>
                              <BootstrapButton
                                href={env.examples}
                                variant="contained"
                                size="small"
                                color="primary"
                              >
                                Examples
                              </BootstrapButton>
                            </div>
                          </div>
                        </CardActions>
                      </CardActionArea>
                    </Card>
                  </Grid>
                );
              } else {
                return <></>;
              }
            });
          })}
        </Grid>
      </main>
    </div>
  );
}
